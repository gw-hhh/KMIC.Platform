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

// 命令事务内按固定顺序对会话、用户、授予和角色加共享锁（FOR SHARE）复核
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
