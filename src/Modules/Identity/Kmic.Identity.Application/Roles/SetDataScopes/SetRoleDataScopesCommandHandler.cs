using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles.SetDataScopes;

/// <summary>
/// 处理 <see cref="SetRoleDataScopesCommand"/>
/// </summary>
public sealed class SetRoleDataScopesCommandHandler(
    IRoleRepository roles,
    IIdentityQueryDb db,
    IResourceAuthorizer<RoleTarget, RoleAdminAction> authorizer,
    DataScopeObjectCatalog catalog,
    IOperatorRecheck recheck,
    IExecutionContext execution) : AuthorizationAdminCommandHandler<SetRoleDataScopesCommand, VersionedResponse<RoleId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override async ValueTask<Result<VersionedResponse<RoleId>>> ExecuteAsync(SetRoleDataScopesCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var unknown = command.Overrides.Keys.Where(code => !catalog.Contains(code)).ToList();
        if (unknown.Count > 0)
        {
            return IdentityErrors.ScopeObjectUnknown.WithDetail($"不认识的范围对象：{string.Join("、", unknown)}");
        }

        var loaded = await RoleAdminLoading.LoadForAsync(roles, db, authorizer, command.Id, command.ExpectedVersion, RoleAdminAction.SetDataScopes, access, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var set = loaded.Value.SetDataScopes(command.DefaultLevel, command.Overrides);
        return set.IsFailure ? set.Error : new VersionedResponse<RoleId>(loaded.Value.Id);
    }
}
