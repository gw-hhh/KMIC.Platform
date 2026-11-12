using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application.Impersonation.End;

/// <summary>
/// 处理 <see cref="EndImpersonationCommand"/>
/// </summary>
public sealed class EndImpersonationCommandHandler(ISessionRepository sessions, IExecutionContext execution, ISecurityEventWriter securityEvents, TimeProvider time) : ICommandHandler<EndImpersonationCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(EndImpersonationCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var access = execution.Access ?? throw new InvalidOperationException("结束模拟身份需要已登录的访问上下文。");
        if (access.ImpersonatorUserId is null)
        {
            return IdentityErrors.NotImpersonating;
        }

        var loaded = await sessions.GetForUpdateAsync(access.SessionId, null, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        loaded.Value.Revoke(RevokeReason.ImpersonationEnded, time.GetUtcNow());
        securityEvents.Write(new SecurityEvent("ImpersonationEnded", SecuritySeverity.Info, null)
        {
            ResourceType = "User",
            ResourceId = access.UserId.Value,
            Details = new Dictionary<string, string>(StringComparer.Ordinal) { ["sessionId"] = access.SessionId.Value.ToString() },
        });
        return Result.Success();
    }
}
