using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Domain.Settings;
using Kmic.Configuration.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Infrastructure.Settings;

// 直接读库获取最新设置值及保存时间（不走缓存）
internal sealed class FreshSettingsReader(IQueryScopeFactory queries, SystemSettingCatalog catalog) : IFreshSettings
{
    public async ValueTask<FreshSetting<T>> GetAsync<T>(CancellationToken ct)
        where T : class, ISystemSetting<T>
    {
        var key = catalog.Get<T>().Key;
        var saved = await queries.RunAsync<ConfigurationQueryDbContext, SavedSetting?>(
            CallOrigin.ModuleCall,
            (db, token) => db.Set<SystemSettingValue>()
                .AsNoTracking()
                .Where(v => v.Key == key)
                .Select(v => new SavedSetting(v.Value, EF.Property<DateTimeOffset?>(v, AuditColumns.UpdatedAt), EF.Property<DateTimeOffset>(v, AuditColumns.CreatedAt)))
                .SingleOrDefaultAsync(token),
            ct);

        // 保存过一次之后才有修改时间，新建时只有创建时间
        return saved is null
            ? new FreshSetting<T>(T.Default, null)
            : new FreshSetting<T>(SettingsSnapshot.ParseOrDefault<T>(saved.Json), saved.UpdatedAt ?? saved.CreatedAt);
    }

    private sealed record SavedSetting(string Json, DateTimeOffset? UpdatedAt, DateTimeOffset CreatedAt);
}
