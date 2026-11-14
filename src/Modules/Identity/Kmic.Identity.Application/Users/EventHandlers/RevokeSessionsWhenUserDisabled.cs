using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users.Events;

namespace Kmic.Identity.Application.Users.EventHandlers;

/// <summary>
/// 用户被禁用：作废他的全部会话，含他作为实际操作人发起的模拟会话
/// </summary>
/// <remarks>
/// 与禁用命令共用事务；请求还会检查用户状态，禁用期间拒绝访问
/// </remarks>
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
