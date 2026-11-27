using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Sessions;

namespace Kmic.Identity.Application.Impersonation.End;

public sealed class EndImpersonationCommandHandler(ISessionRepository sessions, IExecutionContext execution, ISecurityEventWriter securityEvents, TimeProvider time) : ICommandHandler<EndImpersonationCommand>
{
    public async ValueTask<Result> Handle(EndImpersonationCommand command, CancellationToken cancellationToken)
    {
        var access = execution.Access ?? throw new InvalidOperationException("结束模拟身份需要已登录的用户身份。");
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
