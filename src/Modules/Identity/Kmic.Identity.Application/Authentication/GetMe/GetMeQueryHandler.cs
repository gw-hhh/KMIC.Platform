using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Time;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Authentication.GetMe;

/// <summary>
/// 处理 <see cref="GetMeQuery"/>
/// </summary>
public sealed class GetMeQueryHandler(IIdentityQueryDb db, IExecutionContext execution, IOptions<BusinessTimeOptions> businessTime)
    : IQueryHandler<GetMeQuery, MeResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<MeResponse>> Handle(GetMeQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var access = execution.Access ?? throw new InvalidOperationException("查询当前用户需要已登录的访问上下文。");
        var user = await db.Users
            .Where(u => u.Id == access.UserId)
            .Select(u => new { u.Account, u.DisplayName, u.EnglishName, u.OrgUnitId, u.PasswordHash, RoleIds = u.Grants.Select(g => g.RoleId).ToList() })
            .SingleOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            return IdentityErrors.UserNotFound;
        }

        var roles = await db.Roles
            .Where(r => user.RoleIds.Contains(r.Id) && r.Status == RoleStatus.Active)
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.Name)
            .Select(r => new MeRole(r.Id.Value, r.Name))
            .ToListAsync(cancellationToken);
        var current = roles.Find(r => r.Id == access.RoleId.Value)
            ?? new MeRole(access.RoleId.Value, await db.Roles.Where(r => r.Id == access.RoleId).Select(r => r.Name).SingleOrDefaultAsync(cancellationToken) ?? string.Empty);

        return new MeResponse(
            access.UserId.Value,
            user.Account,
            user.DisplayName,
            user.EnglishName,
            user.OrgUnitId?.Value,
            current,
            roles,
            [.. access.Permissions.Order(StringComparer.Ordinal)],
            access.AuthorizationVersion,
            access.PasswordChangeRequired,
            !string.IsNullOrEmpty(user.PasswordHash),
            access.ImpersonatorUserId is not null,
            access.ImpersonatorUserId?.Value,
            access.ImpersonationAllowsWrite,
            businessTime.Value.BusinessTimeZone);
    }
}
