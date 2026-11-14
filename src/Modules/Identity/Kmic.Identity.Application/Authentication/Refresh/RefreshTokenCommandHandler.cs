using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Authentication.Refresh;

/// <summary>
/// 轮换刷新令牌并签发访问令牌
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

        // 锁定会话，使并发刷新依次执行，后到的请求按宽限期规则处理
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

        // 失败会回滚当前事务，接口层须另发命令独立提交会话作废操作
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
