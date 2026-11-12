using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users.Events;

namespace Kmic.Identity.Application.Users.EventHandlers;

/// <summary>
/// 密码变化：管理员重置时作废该用户全部会话；用户自己修改时作废其他会话，当前会话继续
/// </summary>
internal sealed class RevokeSessionsWhenPasswordChanged(ISessionRepository sessions, IExecutionContext execution, TimeProvider time) : IDomainEventHandler<PasswordChanged>
{
    public async ValueTask<Result> HandleAsync(PasswordChanged domainEvent, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        var now = time.GetUtcNow();
        var keep = domainEvent.ByAdministrator ? null : execution.Access?.SessionId;
        var reason = domainEvent.ByAdministrator ? RevokeReason.PasswordReset : RevokeReason.PasswordChanged;
        foreach (var session in await sessions.ListActiveInvolvingAsync(domainEvent.UserId, ct))
        {
            if (session.Id != keep)
            {
                session.Revoke(reason, now);
            }
        }

        return Result.Success();
    }
}
