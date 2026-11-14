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
            // 带版本号但设置值已不存在，按版本冲突处理
            return expectedVersion is null ? (SystemSettingValue?)null : CommonErrors.VersionConflict;
        }

        // 未带版本号表示读取时没有设置值，现在已有值则发生了版本冲突
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
