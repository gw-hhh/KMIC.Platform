using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users.Events;

namespace Kmic.Identity.Application.Users.EventHandlers;

/// <summary>
/// 用户被禁用：作废他的全部会话，含他作为实际操作人发起的模拟会话
/// </summary>
/// <remarks>
/// 和禁用命令在同一个事务里。会话状态只是记录，正确性不依赖它：每个请求都会连同用户状态一起查
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
