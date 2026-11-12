using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Sessions;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Authentication.Refresh;

/// <summary>
/// 处理 <see cref="RevokeReusedSessionCommand"/>
/// </summary>
public sealed class RevokeReusedSessionCommandHandler(
    ISessionRepository sessions,
    IOptions<SessionLifetimeOptions> sessionOptions,
    ISecurityEventWriter securityEvents,
    TimeProvider time) : ICommandHandler<RevokeReusedSessionCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(RevokeReusedSessionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var hash = RefreshTokens.Hash(command.RefreshToken);
        var session = await sessions.FindByRefreshTokenForUpdateAsync(hash, cancellationToken);
        if (session is not null && session.RevokeIfReused(hash, time.GetUtcNow(), sessionOptions.Value.RefreshGrace))
        {
            securityEvents.Write(new SecurityEvent("RefreshTokenReused", SecuritySeverity.High, null) { UserId = session.UserId.Value });
        }

        return Result.Success();
    }
}
