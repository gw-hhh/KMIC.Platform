using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application.Authentication.MySessions;

public sealed class RevokeMySessionCommandHandler(ISessionRepository sessions, IExecutionContext execution, ISecurityEventWriter securityEvents, TimeProvider time) : ICommandHandler<RevokeMySessionCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(RevokeMySessionCommand command, CancellationToken cancellationToken)
    {
        var access = execution.Access ?? throw new InvalidOperationException("结束登录会话需要已登录的访问上下文。");
        var loaded = await sessions.GetForUpdateAsync(command.SessionId, null, cancellationToken);

        // 他人会话和模拟会话统一按不存在处理，避免泄露会话信息
        if (loaded.IsFailure || loaded.Value.UserId != access.UserId || loaded.Value.IsImpersonation)
        {
            return IdentityErrors.SessionNotFound;
        }

        if (loaded.Value.Status == SessionStatus.Active)
        {
            loaded.Value.Revoke(RevokeReason.Logout, time.GetUtcNow());
            securityEvents.Write(new SecurityEvent("SessionRevoked", SecuritySeverity.Info, nameof(RevokeReason.Logout)) { ResourceType = "Session", ResourceId = command.SessionId.Value });
        }

        return Result.Success();
    }
}
