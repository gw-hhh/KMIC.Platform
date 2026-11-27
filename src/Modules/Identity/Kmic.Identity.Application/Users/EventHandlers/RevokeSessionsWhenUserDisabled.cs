using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users.Events;

namespace Kmic.Identity.Application.Users.EventHandlers;

// 用户禁用后作废其全部有效会话（含其发起的模拟会话）
internal sealed class RevokeSessionsWhenUserDisabled(ISessionRepository sessions, TimeProvider time) : IDomainEventHandler<UserDisabled>
{
    public async ValueTask<Result> HandleAsync(UserDisabled domainEvent, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var now = time.GetUtcNow();
        foreach (var session in await sessions.ListActiveInvolvingAsync(domainEvent.UserId, ct))
        {
            session.Revoke(RevokeReason.UserDisabled, now);
        }

        return Result.Success();
    }
}
