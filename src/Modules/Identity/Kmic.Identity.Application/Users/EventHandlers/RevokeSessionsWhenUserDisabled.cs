using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users.Events;

namespace Kmic.Identity.Application.Users.EventHandlers;

/// <summary>
/// 用户被禁用时作废该用户的全部会话，包括其作为实际操作人发起的模拟会话
/// </summary>
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
