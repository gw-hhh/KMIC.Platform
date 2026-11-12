using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application.Authentication.Logout;

/// <summary>
/// 处理 <see cref="LogoutCommand"/>
/// </summary>
public sealed class LogoutCommandHandler(ISessionRepository sessions, IExecutionContext execution, ISecurityEventWriter securityEvents, TimeProvider time)
    : ICommandHandler<LogoutCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var access = execution.Access ?? throw new InvalidOperationException("退出登录需要已登录的访问上下文。");
        var loaded = await sessions.GetForUpdateAsync(access.SessionId, null, cancellationToken);
        if (loaded.IsFailure)
        {
            // 会话已经不存在：退出的目的已经达到
            return Result.Success();
        }

        loaded.Value.Revoke(RevokeReason.Logout, time.GetUtcNow());
        securityEvents.Write(new SecurityEvent("SessionRevoked", SecuritySeverity.Info, nameof(RevokeReason.Logout)));
        return Result.Success();
    }
}
