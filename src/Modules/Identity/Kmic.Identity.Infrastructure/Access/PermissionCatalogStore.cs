using Kmic.Identity.Application.Catalog;
using Kmic.Identity.Infrastructure.Persistence;
using Kmic.Identity.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Infrastructure.Access;

/// <summary>
/// <see cref="IPermissionCatalogStore" /> 的实现：用写上下文，和命令同一个事务，随命令一起保存
/// </summary>
internal sealed class PermissionCatalogStore(IdentityDbContext context) : IPermissionCatalogStore
{
    private const long SyncLockKey = 0x4B4D_4943_0003;
    private const string StateName = "Permissions";

    public async Task LockSyncAsync(CancellationToken ct) =>
        await context.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({SyncLockKey})", ct);

    public async Task<string?> GetHashAsync(CancellationToken ct) =>
        await context.Set<CatalogStateRow>().Where(s => s.Name == StateName).Select(s => s.ContentHash).SingleOrDefaultAsync(ct);

    public async Task SaveHashAsync(string hash, DateTimeOffset now, CancellationToken ct)
    {
        var state = await context.Set<CatalogStateRow>().SingleOrDefaultAsync(s => s.Name == StateName, ct);
        if (state is null)
        {
            context.Set<CatalogStateRow>().Add(new CatalogStateRow { Name = StateName, ContentHash = hash, SyncedAt = now });
        }
        else
        {
            state.ContentHash = hash;
            state.SyncedAt = now;
        }
    }

    public async Task<CatalogChanges> ApplyAsync(CatalogContent content, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(content);

        var (permissionsAdded, permissionsUpdated, permissionsRetired, renames) = await ApplyPermissionsAsync(content, now, ct);
        var (scopeObjectsAdded, scopeObjectsRetired) = await ApplyScopeObjectsAsync(content, now, ct);
        return new CatalogChanges(permissionsAdded, permissionsUpdated, permissionsRetired, scopeObjectsAdded, scopeObjectsRetired, renames);
    }

    public async Task<IReadOnlyList<string>> RetiredPermissionCodesAsync(DateTimeOffset retiredBefore, CancellationToken ct) =>
        await context.Set<PermissionRow>().Where(p => p.IsRetired && p.RetiredAt < retiredBefore).Select(p => p.Code).ToListAsync(ct);

    public async Task<IReadOnlyList<string>> RetiredScopeObjectCodesAsync(DateTimeOffset retiredBefore, CancellationToken ct) =>
        await context.Set<DataScopeObjectRow>().Where(s => s.IsRetired && s.RetiredAt < retiredBefore).Select(s => s.Code).ToListAsync(ct);

    public async Task DeleteRetiredAsync(IReadOnlyCollection<string> permissionCodes, IReadOnlyCollection<string> scopeObjectCodes, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(permissionCodes);
        ArgumentNullException.ThrowIfNull(scopeObjectCodes);

        await context.Set<PermissionRow>().Where(p => p.IsRetired && permissionCodes.Contains(p.Code)).ExecuteDeleteAsync(ct);
        await context.Set<DataScopeObjectRow>().Where(s => s.IsRetired && scopeObjectCodes.Contains(s.Code)).ExecuteDeleteAsync(ct);
    }

    private async Task<(int Added, int Updated, int Retired, Dictionary<string, string> Renames)> ApplyPermissionsAsync(CatalogContent content, DateTimeOffset now, CancellationToken ct)
    {
        var rows = await context.Set<PermissionRow>().ToListAsync(ct);
        var byCode = rows.ToDictionary(r => r.Code, StringComparer.Ordinal);
        var touched = new HashSet<PermissionRow>();
        var renames = RenameInPlace(content, byCode, touched);
        var added = Upsert(content, byCode, touched);

        var declared = content.Permissions.Select(p => p.Code).ToHashSet(StringComparer.Ordinal);
        var gone = byCode.Values.Where(r => !declared.Contains(r.Code) && !r.IsRetired).ToList();
        foreach (var row in gone)
        {
            row.IsRetired = true;
            row.RetiredAt = now;
        }

        return (added, touched.Count, gone.Count, renames);
    }

    // 声明了改名：数据库里的旧编码原地改成新编码，行的 ID 不变；新编码已经有行就不动，旧行按“代码里没有了”处理
    private static Dictionary<string, string> RenameInPlace(CatalogContent content, Dictionary<string, PermissionRow> byCode, HashSet<PermissionRow> touched)
    {
        var renames = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var desired in content.Permissions.Where(p => p.RenamedFrom is not null))
        {
            renames[desired.RenamedFrom!] = desired.Code;
            if (!byCode.ContainsKey(desired.Code) && byCode.Remove(desired.RenamedFrom!, out var renamed))
            {
                renamed.Code = desired.Code;
                byCode[desired.Code] = renamed;
                touched.Add(renamed);
            }
        }

        return renames;
    }

    // 新增没有的，更新变了的（退役的重新出现时一并恢复）；返回新增的数量
    private int Upsert(CatalogContent content, Dictionary<string, PermissionRow> byCode, HashSet<PermissionRow> touched)
    {
        var added = 0;
        foreach (var desired in content.Permissions)
        {
            if (!byCode.TryGetValue(desired.Code, out var row))
            {
                context.Set<PermissionRow>().Add(new PermissionRow
                {
                    Id = Guid.CreateVersion7(),
                    Code = desired.Code,
                    Module = desired.Module,
                    GroupName = desired.Group,
                    DisplayName = desired.DisplayName,
                    IsSensitive = desired.Sensitive,
                });
                added++;
                continue;
            }

            var changed = !string.Equals(row.Module, desired.Module, StringComparison.Ordinal)
                || !string.Equals(row.GroupName, desired.Group, StringComparison.Ordinal)
                || !string.Equals(row.DisplayName, desired.DisplayName, StringComparison.Ordinal)
                || row.IsSensitive != desired.Sensitive
                || row.IsRetired;
            if (changed)
            {
                row.Module = desired.Module;
                row.GroupName = desired.Group;
                row.DisplayName = desired.DisplayName;
                row.IsSensitive = desired.Sensitive;
                row.IsRetired = false;
                row.RetiredAt = null;
                touched.Add(row);
            }
        }

        return added;
    }

    private async Task<(int Added, int Retired)> ApplyScopeObjectsAsync(CatalogContent content, DateTimeOffset now, CancellationToken ct)
    {
        var rows = await context.Set<DataScopeObjectRow>().ToListAsync(ct);
        var byCode = rows.ToDictionary(r => r.Code, StringComparer.Ordinal);
        var added = 0;
        foreach (var desired in content.ScopeObjects)
        {
            if (!byCode.TryGetValue(desired.Code, out var row))
            {
                context.Set<DataScopeObjectRow>().Add(new DataScopeObjectRow
                {
                    Id = Guid.CreateVersion7(),
                    Code = desired.Code,
                    Module = desired.Module,
                    DisplayName = desired.DisplayName,
                });
                added++;
                continue;
            }

            row.Module = desired.Module;
            row.DisplayName = desired.DisplayName;
            row.IsRetired = false;
            row.RetiredAt = null;
        }

        var declared = content.ScopeObjects.Select(s => s.Code).ToHashSet(StringComparer.Ordinal);
        var gone = rows.Where(r => !declared.Contains(r.Code) && !r.IsRetired).ToList();
        foreach (var row in gone)
        {
            row.IsRetired = true;
            row.RetiredAt = now;
        }

        return (added, gone.Count);
    }
}
