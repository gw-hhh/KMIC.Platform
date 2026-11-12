using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Authentication.Refresh;

/// <summary>
/// 处理 <see cref="RefreshTokenCommand"/>
/// </summary>
public sealed class RefreshTokenCommandHandler(
    ISessionRepository sessions,
    IUserRepository users,
    IAccessTokenIssuer tokens,
    IOptions<SessionLifetimeOptions> sessionOptions,
    TimeProvider time) : ICommandHandler<RefreshTokenCommand, AuthTokens>
{
    /// <inheritdoc />
    public async ValueTask<Result<AuthTokens>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var presentedHash = RefreshTokens.Hash(command.RefreshToken);

        // 按令牌找到会话并锁住：两个并发刷新请求在这里排队，后到的落入宽限期判断
        var session = await sessions.FindByRefreshTokenForUpdateAsync(presentedHash, cancellationToken);
        if (session is null)
        {
            return IdentityErrors.InvalidRefreshToken;
        }

        var now = time.GetUtcNow();
        var options = sessionOptions.Value;
        var newToken = RefreshTokens.Generate();
        var rotation = session.Rotate(
            presentedHash,
            RefreshTokens.Hash(newToken),
            now,
            session.ClientType == ClientType.Web ? options.WebRefreshLifetime : options.AppRefreshLifetime,
            options.RefreshGrace);

        // 重用：这个命令随失败回滚，作废会话由接口层随后发的命令在独立事务里完成
        if (rotation.Outcome == RotationOutcome.Reused)
        {
            return IdentityErrors.RefreshTokenReused;
        }

        if (rotation.Outcome != RotationOutcome.Rotated)
        {
            return IdentityErrors.InvalidRefreshToken;
        }

        var loaded = await users.GetForUpdateAsync(session.UserId, null, cancellationToken);
        if (loaded.IsFailure || !loaded.Value.CanLogin)
        {
            return IdentityErrors.InvalidRefreshToken;
        }

        var accessToken = tokens.Issue(session.UserId, session.Id, session.CurrentRoleId, ClientKinds.Claim(session.ClientType), now);
        return new AuthTokens(accessToken.Value, "Bearer", (int)(accessToken.ExpiresAt - now).TotalSeconds, newToken, loaded.Value.PasswordChangeRequired);
    }
}
