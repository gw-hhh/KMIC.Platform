using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application.Authentication.Logout;

public sealed class LogoutCommandHandler(ISessionRepository sessions, IExecutionContext execution, ISecurityEventWriter securityEvents, TimeProvider time)
    : ICommandHandler<LogoutCommand>
{
    public async ValueTask<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var access = execution.Access ?? throw new InvalidOperationException("退出登录需要已登录的用户身份。");
        var loaded = await sessions.GetForUpdateAsync(access.SessionId, null, cancellationToken);
        if (loaded.IsFailure)
        {
            // 会话已不存在，视为退出成功
            return Result.Success();
        }

        loaded.Value.Revoke(RevokeReason.Logout, time.GetUtcNow());
        securityEvents.Write(new SecurityEvent("SessionRevoked", SecuritySeverity.Info, nameof(RevokeReason.Logout)));
        return Result.Success();
    }
}
