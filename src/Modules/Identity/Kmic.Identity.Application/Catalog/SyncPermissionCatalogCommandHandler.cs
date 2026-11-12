using System.Globalization;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Catalog;

/// <summary>
/// 处理 <see cref="SyncPermissionCatalogCommand"/>
/// </summary>
public sealed class SyncPermissionCatalogCommandHandler(
    PermissionCatalog permissions,
    DataScopeObjectCatalog scopeObjects,
    IPermissionCatalogStore store,
    IRoleRepository roles,
    ISecurityEventWriter securityEvents,
    TimeProvider time) : ICommandHandler<SyncPermissionCatalogCommand, CatalogSyncResult>
{
    /// <inheritdoc />
    public async ValueTask<Result<CatalogSyncResult>> Handle(SyncPermissionCatalogCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var content = CatalogContent.From(permissions, scopeObjects);
        var hash = content.Hash();
        await store.LockSyncAsync(cancellationToken);
        if (string.Equals(await store.GetHashAsync(cancellationToken), hash, StringComparison.Ordinal))
        {
            return new CatalogSyncResult(false, null);
        }

        var now = time.GetUtcNow();
        var changes = await store.ApplyAsync(content, now, cancellationToken);
        await RenameOnRolesAsync(changes.Renames, cancellationToken);
        foreach (var role in await roles.ListGrantingAllAsync(cancellationToken))
        {
            role.SyncCatalog(hash);
        }

        await store.SaveHashAsync(hash, now, cancellationToken);
        securityEvents.Write(new SecurityEvent("PermissionCatalogSynced", SecuritySeverity.Info, null)
        {
            Details = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["permissionsAdded"] = Count(changes.PermissionsAdded),
                ["permissionsUpdated"] = Count(changes.PermissionsUpdated),
                ["permissionsRetired"] = Count(changes.PermissionsRetired),
                ["scopeObjectsAdded"] = Count(changes.ScopeObjectsAdded),
                ["scopeObjectsRetired"] = Count(changes.ScopeObjectsRetired),
            },
        });
        return new CatalogSyncResult(true, changes);
    }

    private static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);

    // 角色上配置的是权限点编码：声明了改名，就把所有角色上的旧编码换成新编码，角色的授权版本随之递增
    private async Task RenameOnRolesAsync(IReadOnlyDictionary<string, string> renames, CancellationToken ct)
    {
        if (renames.Count == 0)
        {
            return;
        }

        foreach (var role in await roles.ListWithPermissionsAsync([.. renames.Keys], ct))
        {
            role.RenamePermissions(renames);
        }
    }
}
