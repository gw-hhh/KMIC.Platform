using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Roles.Get;

/// <summary>
/// 处理 <see cref="GetRoleQuery"/>
/// </summary>
public sealed class GetRoleQueryHandler(
    IIdentityQueryDb db,
    IExecutionContext execution,
    PermissionCatalog permissions,
    DataScopeObjectCatalog scopeObjects,
    IAllowedActions<RoleTarget> allowedActions) : IQueryHandler<GetRoleQuery, RoleDetailResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<RoleDetailResponse>> Handle(GetRoleQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var access = execution.Access ?? throw new InvalidOperationException("查询角色需要已登录的访问上下文。");
        var found = await db.Roles
            .Where(r => r.Id == query.Id)
            .Select(r => new
            {
                Role = r,
                Codes = r.Permissions.Select(p => p.PermissionCode).ToList(),
                Scopes = r.DataScopes.Select(s => new { s.ScopeObject, s.Level }).ToList(),
                Holders = db.Users.Count(u => u.Grants.Any(g => g.RoleId == r.Id)),
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (found is null)
        {
            return IdentityErrors.RoleNotFound;
        }

        var role = found.Role;
        var operatorProtected = await RoleAdminLoading.OperatorIsProtectedAsync(db, access, cancellationToken);
        var definitions = permissions.All.ToDictionary(p => p.Code, StringComparer.Ordinal);
        var objects = scopeObjects.All.ToDictionary(o => o.Code, StringComparer.Ordinal);
        return new RoleDetailResponse(
            role.Id,
            role.ConcurrencyVersion,
            role.Name,
            role.Description,
            role.Status,
            role.IsProtected,
            role.IsBuiltIn,
            role.SortOrder,
            role.DefaultScopeLevel,
            [.. found.Codes.Order(StringComparer.Ordinal).Select(code => new RolePermissionItem(code, definitions.GetValueOrDefault(code)?.DisplayName, !definitions.ContainsKey(code)))],
            [.. found.Scopes.OrderBy(s => s.ScopeObject, StringComparer.Ordinal).Select(s => new RoleDataScopeItem(s.ScopeObject, objects.GetValueOrDefault(s.ScopeObject)?.DisplayName, s.Level))],
            found.Holders,
            allowedActions.For(new RoleTarget(role, operatorProtected)));
    }
}
