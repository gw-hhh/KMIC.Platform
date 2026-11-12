using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Users.Sessions;

/// <summary>
/// 处理 <see cref="ListUserSessionsQuery"/>
/// </summary>
public sealed class ListUserSessionsQueryHandler(IIdentityQueryDb db, TimeProvider time) : IQueryHandler<ListUserSessionsQuery, IReadOnlyList<UserSessionItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<IReadOnlyList<UserSessionItem>>> Handle(ListUserSessionsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!await db.Users.AnyAsync(u => u.Id == query.Id, cancellationToken))
        {
            return IdentityErrors.UserNotFound;
        }

        var now = time.GetUtcNow();
        List<UserSessionItem> items = await db.Sessions
            .Where(s => (s.UserId == query.Id || s.ImpersonatorUserId == query.Id) && s.Status == SessionStatus.Active && s.AbsoluteExpiresAt > now)
            .OrderByDescending(s => s.LastRefreshedAt)
            .Select(s => new UserSessionItem(
                s.Id, s.UserId, s.ImpersonatorUserId, s.ClientType, s.DeviceName, s.LoginIp, EF.Property<DateTimeOffset>(s, AuditColumns.CreatedAt), s.LastRefreshedAt, s.RefreshExpiresAt))
            .ToListAsync(cancellationToken);
        Result<IReadOnlyList<UserSessionItem>> result = items;
        return result;
    }
}
