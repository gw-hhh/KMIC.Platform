using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application.Authentication.MySessions;

/// <summary>
/// 处理 <see cref="RevokeMySessionCommand"/>
/// </summary>
public sealed class RevokeMySessionCommandHandler(ISessionRepository sessions, IExecutionContext execution, ISecurityEventWriter securityEvents, TimeProvider time) : ICommandHandler<RevokeMySessionCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(RevokeMySessionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var access = execution.Access ?? throw new InvalidOperationException("结束登录设备需要已登录的访问上下文。");
        var loaded = await sessions.GetForUpdateAsync(command.SessionId, null, cancellationToken);

        // 别人的会话、模拟会话和不存在的会话一样处理，不让人探测别的用户有哪些会话
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
