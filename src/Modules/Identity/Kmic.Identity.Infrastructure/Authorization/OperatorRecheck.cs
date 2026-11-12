using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users;
using Kmic.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Infrastructure.Authorization;

/// <summary>
/// 操作者重查：在命令事务里按固定顺序（会话、用户、授予、角色）加共享锁并检查
/// </summary>
/// <remarks>
/// 不检查登录限制：锁定只影响新的登录尝试，不是用户状态
/// </remarks>
internal sealed class OperatorRecheck(IdentityDbContext db, TimeProvider time) : IOperatorRecheck
{
    public async Task<Result> CheckAsync(IAccessContext access, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(access);

        var session = await db.Database.SqlQuery<SessionLockRow>(
            $"""SELECT "Status", "AbsoluteExpiresAt" FROM "Identity"."Session" WHERE "Id" = {access.SessionId.Value} FOR SHARE""").SingleOrDefaultAsync(ct);
        if (session is null || !string.Equals(session.Status, SessionStatus.Active.Code, StringComparison.Ordinal) || time.GetUtcNow() >= session.AbsoluteExpiresAt)
        {
            return IdentityErrors.AuthorizationChanged;
        }

        var user = await db.Database.SqlQuery<StatusLockRow>(
            $"""SELECT "Status" FROM "Identity"."User" WHERE "Id" = {access.UserId.Value} FOR SHARE""").SingleOrDefaultAsync(ct);
        if (user is null || !string.Equals(user.Status, UserStatus.Active.Code, StringComparison.Ordinal))
        {
            return IdentityErrors.AuthorizationChanged;
        }

        var granted = await db.Database.SqlQuery<Guid>(
            $"""SELECT "Id" AS "Value" FROM "Identity"."UserRoleGrant" WHERE "UserId" = {access.UserId.Value} AND "RoleId" = {access.RoleId.Value} FOR SHARE""").AnyAsync(ct);
        if (!granted)
        {
            return IdentityErrors.AuthorizationChanged;
        }

        var role = await db.Database.SqlQuery<RoleLockRow>(
            $"""SELECT "Status", "ConcurrencyVersion" FROM "Identity"."Role" WHERE "Id" = {access.RoleId.Value} FOR SHARE""").SingleOrDefaultAsync(ct);
        return role is null
            || !string.Equals(role.Status, RoleStatus.Active.Code, StringComparison.Ordinal)
            || role.ConcurrencyVersion != access.AuthorizationVersion
                ? IdentityErrors.AuthorizationChanged
                : Result.Success();
    }

    private sealed class SessionLockRow
    {
        public required string Status { get; init; }

        public DateTimeOffset AbsoluteExpiresAt { get; init; }
    }

    private sealed class StatusLockRow
    {
        public required string Status { get; init; }
    }

    private sealed class RoleLockRow
    {
        public required string Status { get; init; }

        public long ConcurrencyVersion { get; init; }
    }
}
