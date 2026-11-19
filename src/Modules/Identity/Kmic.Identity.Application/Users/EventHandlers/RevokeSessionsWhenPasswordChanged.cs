using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users.Events;

namespace Kmic.Identity.Application.Users.EventHandlers;

/// <summary>
/// 密码变更后作废相关会话
/// </summary>
/// <remarks>
/// 管理员重置时全部作废，用户自己改密码时保留当前会话；包括相关的模拟会话
/// </remarks>
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
