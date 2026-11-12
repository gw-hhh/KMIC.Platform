using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Roles;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Catalog;

/// <summary>
/// 处理 <see cref="PurgeRetiredPermissionsCommand"/>
/// </summary>
public sealed class PurgeRetiredPermissionsCommandHandler(
    IPermissionCatalogStore store,
    IRoleRepository roles,
    IOptions<CatalogOptions> options,
    TimeProvider time) : ICommandHandler<PurgeRetiredPermissionsCommand, ScheduledJobOutcome>
{
    /// <inheritdoc />
    public async ValueTask<Result<ScheduledJobOutcome>> Handle(PurgeRetiredPermissionsCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var cutoff = time.GetUtcNow() - options.Value.RetiredRetention;
        var permissionCodes = await store.RetiredPermissionCodesAsync(cutoff, cancellationToken);
        var scopeObjects = await store.RetiredScopeObjectCodesAsync(cutoff, cancellationToken);
        if (permissionCodes.Count == 0 && scopeObjects.Count == 0)
        {
            return new ScheduledJobOutcome(0);
        }

        if (permissionCodes.Count > 0)
        {
            foreach (var role in await roles.ListWithPermissionsAsync(permissionCodes, cancellationToken))
            {
                role.RemovePermissions(permissionCodes);
            }
        }

        if (scopeObjects.Count > 0)
        {
            foreach (var role in await roles.ListWithDataScopeOverridesAsync(scopeObjects, cancellationToken))
            {
                role.RemoveDataScopeOverrides(scopeObjects);
            }
        }

        await store.DeleteRetiredAsync(permissionCodes, scopeObjects, cancellationToken);
        return new ScheduledJobOutcome(permissionCodes.Count + scopeObjects.Count);
    }
}
