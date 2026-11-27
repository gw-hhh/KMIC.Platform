using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Users;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Authorization.Explain;

public sealed class ExplainAuthorizationQueryHandler(IIdentityQueryDb db, IAccessContextLoader loader, DataScopeObjectCatalog scopeObjects)
    : IQueryHandler<ExplainAuthorizationQuery, AuthorizationExplanation>
{
    public async ValueTask<Result<AuthorizationExplanation>> Handle(ExplainAuthorizationQuery query, CancellationToken cancellationToken)
    {
        var scopeObject = query.ScopeObject is { } code ? scopeObjects.All.FirstOrDefault(s => string.Equals(s.Code, code, StringComparison.Ordinal)) : null;
        if (query.ScopeObject is not null && scopeObject is null)
        {
            return IdentityErrors.ScopeObjectUnknown;
        }

        var user = await db.Users
            .Where(u => u.Id == query.UserId)
            .Select(u => new { u.DisplayName, u.Status, u.Kind, Granted = u.Grants.Any(g => g.RoleId == query.RoleId) })
            .SingleOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            return IdentityErrors.UserNotFound;
        }

        var role = await db.Roles
            .Where(r => r.Id == query.RoleId)
            .Select(r => new { r.Name, r.Status, r.ConcurrencyVersion })
            .SingleOrDefaultAsync(cancellationToken);
        if (role is null)
        {
            return IdentityErrors.RoleNotFound;
        }

        // 先核查不可用原因；可用时按常规登录流程加载访问上下文
        var reason = UnusableReason(user.Status, user.Kind, user.Granted, role.Status);
        var access = reason is null ? await loader.LoadForJobAsync(query.UserId, query.RoleId, cancellationToken) : null;
        if (access is null)
        {
            return new AuthorizationExplanation(
                query.UserId, user.DisplayName, query.RoleId, role.Name, role.ConcurrencyVersion, false, reason ?? "用户权限加载失败，请稍后重试", [], null);
        }

        return new AuthorizationExplanation(
            query.UserId,
            user.DisplayName,
            query.RoleId,
            role.Name,
            access.AuthorizationVersion,
            true,
            null,
            [.. access.Permissions.Order(StringComparer.Ordinal)],
            scopeObject is null ? null : await ExplainAsync(access, scopeObject, cancellationToken));
    }

    // 与访问上下文加载器的判断一致：普通用户、已启用、已获授该角色、角色已启用
    private static string? UnusableReason(UserStatus status, UserKind kind, bool granted, RoleStatus roleStatus)
    {
        if (kind != UserKind.Normal)
        {
            return "系统账号不能登录";
        }

        if (status != UserStatus.Active)
        {
            return "用户已禁用";
        }

        if (!granted)
        {
            return "用户未被授予该角色";
        }

        return roleStatus == RoleStatus.Active ? null : "角色已停用";
    }

    private async Task<DataScopeExplanation> ExplainAsync(IAccessContext access, DataScopeObjectDefinition scopeObject, CancellationToken ct)
    {
        var grant = access.DataScopeGrant;
        var level = DataScopeRules.LevelOf(grant, scopeObject.Code);
        var byOrgUnit = level is ScopeLevel.OrgUnit or ScopeLevel.OrgUnitAndBelow;
        var anchors = byOrgUnit ? DataScopeRules.AnchorsOf(access) : [];
        var names = await UserNames.OrgUnitsAsync(db, anchors, ct);

        return new DataScopeExplanation(
            scopeObject.Code,
            scopeObject.DisplayName,
            RoleScopeLevel.FromCode(level.ToString()),
            grant.Overrides.ContainsKey(scopeObject.Code),
            [.. anchors.Select(a => new ExplainedOrgUnit(a, names.GetValueOrDefault(a))).OrderBy(a => a.Name, StringComparer.Ordinal)],
            byOrgUnit && grant.Anchors.Count > 0,
            byOrgUnit ? DataScopeRules.OrgUnitsOf(access, level).Count : null);
    }
}
