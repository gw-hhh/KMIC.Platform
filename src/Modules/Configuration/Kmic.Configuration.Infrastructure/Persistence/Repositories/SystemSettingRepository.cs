using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Domain.Errors;
using Kmic.Configuration.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Infrastructure.Persistence.Repositories;

internal sealed class SystemSettingRepository(ConfigurationDbContext context)
    : EfRepository<ConfigurationDbContext, SystemSettingValue, SystemSettingValueId>(context), ISystemSettingRepository
{
    protected override Error NotFound => ConfigurationErrors.SettingNotFound;

    public async Task<Result<SystemSettingValue?>> GetByKeyForUpdateAsync(string key, long? expectedVersion, CancellationToken ct)
    {
        var id = await Context.Set<SystemSettingValue>()
            .Where(v => v.Key == key)
            .Select(v => (SystemSettingValueId?)v.Id)
            .SingleOrDefaultAsync(ct);
        if (id is null)
        {
            // 调用方说它看到的是某个版本，值行却已经没有了：别人在这期间恢复了默认值
            return expectedVersion is null ? (SystemSettingValue?)null : CommonErrors.VersionConflict;
        }

        // 调用方没给版本号，说明它看到的是默认值，而现在已经有人改过了
        if (expectedVersion is null)
        {
            return CommonErrors.VersionConflict;
        }

        var loaded = await GetForUpdateAsync(id.Value, expectedVersion, ct);
        return loaded.IsFailure ? loaded.Error : loaded.Value;
    }

    public void AddChange(SystemSettingChange change)
    {
        ArgumentNullException.ThrowIfNull(change);

        Context.Set<SystemSettingChange>().Add(change);
    }
}
