using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authentication;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Users.Get;

/// <summary>
/// 处理 <see cref="GetUserQuery"/>
/// </summary>
public sealed class GetUserQueryHandler(
    IIdentityQueryDb db,
    IExecutionContext execution,
    IAllowedActions<UserTarget> allowedActions,
    ILoginThrottleStore throttle,
    TimeProvider time) : IQueryHandler<GetUserQuery, UserDetailResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<UserDetailResponse>> Handle(GetUserQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var access = execution.Access ?? throw new InvalidOperationException("查询用户需要已登录的访问上下文。");
        var found = await db.Users
            .Where(u => u.Id == query.Id)
            .Select(u => new
            {
                User = u,
                Grants = u.Grants.Select(g => new { g.RoleId, g.GrantedAt, g.GrantedBy, Anchors = g.Anchors.Select(a => a.OrgUnitId).ToList() }).ToList(),
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (found is null)
        {
            return IdentityErrors.UserNotFound;
        }

        var user = found.User;
        var now = time.GetUtcNow();
        var roleIds = found.Grants.Select(g => g.RoleId).ToList();
        var roles = await UserNames.RolesAsync(db, roleIds, cancellationToken);
        var orgUnits = await UserNames.OrgUnitsAsync(db, found.Grants.SelectMany(g => g.Anchors).Concat(user.OrgUnitId is { } own ? [own] : []), cancellationToken);
        var grantors = await UserNames.UsersAsync(db, found.Grants.Select(g => g.GrantedBy), cancellationToken);
        var sessions = await db.Sessions.CountAsync(s => (s.UserId == user.Id || s.ImpersonatorUserId == user.Id) && s.Status == SessionStatus.Active && s.AbsoluteExpiresAt > now, cancellationToken);
        var remaining = LoginThrottlePolicy.Remaining(await throttle.GetAsync(LoginNames.Hash(user.Account), cancellationToken), now);
        var target = await UserTargets.BuildAsync(db, user, roleIds, access, cancellationToken);

        return new UserDetailResponse(
            user.Id,
            user.ConcurrencyVersion,
            user.Account,
            user.DisplayName,
            user.EnglishName,
            user.EmployeeNo,
            user.Mobile,
            user.Email,
            user.Status,
            user.OrgUnitId,
            user.OrgUnitId is { } unit ? orgUnits.GetValueOrDefault(unit) : null,
            !string.IsNullOrEmpty(user.PasswordHash),
            user.PasswordChangeRequired,
            user.PasswordChangedAt,
            remaining is { } wait ? now + wait : null,
            [
                .. found.Grants
                    .Select(g => new UserGrantItem(
                        g.RoleId,
                        roles.GetValueOrDefault(g.RoleId)?.Name,
                        roles.GetValueOrDefault(g.RoleId)?.IsActive ?? false,
                        roles.GetValueOrDefault(g.RoleId)?.IsProtected ?? false,
                        g.GrantedAt,
                        g.GrantedBy,
                        grantors.GetValueOrDefault(g.GrantedBy),
                        [.. g.Anchors.Select(a => new UserAnchorItem(a, orgUnits.GetValueOrDefault(a))).OrderBy(a => a.Name, StringComparer.Ordinal)]))
                    .OrderBy(g => g.RoleName, StringComparer.Ordinal),
            ],
            sessions,
            allowedActions.For(target));
    }
}
