using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Sessions;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Authentication.Login;

/// <summary>
/// 处理 <see cref="LoginCommand"/>
/// </summary>
/// <remarks>
/// 登录失败要计数，计数在独立事务里提交，命令回滚也不丢。等待与锁定期间不校验密码、不累加次数
/// </remarks>
public sealed class LoginCommandHandler(
    IUserRepository users,
    IIdentityCommandReads db,
    ISessionRepository sessions,
    IPasswordService passwords,
    ILoginThrottleStore throttles,
    IAccessTokenIssuer tokens,
    IOptions<SessionLifetimeOptions> sessionOptions,
    ISecurityEventWriter securityEvents,
    TimeProvider time) : ICommandHandler<LoginCommand, AuthTokens>
{
    /// <inheritdoc />
    public async ValueTask<Result<AuthTokens>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var now = time.GetUtcNow();
        var account = command.Account.Trim();
        var key = LoginNames.Hash(account);

        // 限制检查放在“按账号查用户”之前：存在与不存在的登录名行为一致
        var state = await throttles.GetAsync(key, cancellationToken);
        if (LoginThrottlePolicy.Remaining(state, now) is { } wait)
        {
            securityEvents.Write(Event("LoginFailed", SecuritySeverity.Warning, "Throttled", account, command));
            return IdentityErrors.LoginThrottled.WithMetadata(ErrorMetadataKeys.RetryAfterSeconds, (int)Math.Ceiling(wait.TotalSeconds));
        }

        var user = await users.FindByAccountAsync(account, cancellationToken);
        var failure = CheckCredentials(user, command.Password);
        if (failure is not null)
        {
            await RecordFailureAsync(key, account, failure, command, now, cancellationToken);
            return IdentityErrors.InvalidCredentials;
        }

        var verified = user!;
        var role = await ChooseRoleAsync(verified, cancellationToken);
        if (role is null)
        {
            securityEvents.Write(Event("LoginFailed", SecuritySeverity.Warning, "NoRole", account, command) with { UserId = verified.Id.Value });
            return IdentityErrors.NoRoleAvailable;
        }

        var clientType = ClientKinds.Parse(command.ClientType);
        var refreshToken = RefreshTokens.Generate();
        var options = sessionOptions.Value;
        var web = clientType == ClientType.Web;
        var session = Session.Start(
            verified.Id,
            role.Id,
            clientType,
            Truncate(command.UserAgent, 200),
            command.ClientIp,
            RefreshTokens.Hash(refreshToken),
            now,
            web ? options.WebRefreshLifetime : options.AppRefreshLifetime,
            web ? options.WebAbsoluteLifetime : options.AppAbsoluteLifetime);
        sessions.Add(session);
        verified.RecordRoleUsed(role.Id);

        await throttles.ClearAsync(key, cancellationToken);
        securityEvents.Write(Event("LoginSucceeded", SecuritySeverity.Info, null, account, command) with { UserId = verified.Id.Value });

        var accessToken = tokens.Issue(verified.Id, session.Id, role.Id, ClientKinds.Claim(clientType), now);
        return new AuthTokens(accessToken.Value, "Bearer", (int)(accessToken.ExpiresAt - now).TotalSeconds, refreshToken, verified.PasswordChangeRequired);
    }

    // 返回失败原因；通过则返回空。通过但哈希过时时顺手升级
    private string? CheckCredentials(User? user, string password)
    {
        if (user is null)
        {
            // 账号不存在也做一次哈希，使耗时和密码错误接近
            passwords.Burn(password);
            return "UnknownAccount";
        }

        var check = passwords.Verify(user, password);
        if (check == PasswordCheck.Failed)
        {
            return "WrongPassword";
        }

        if (user.Kind == UserKind.System)
        {
            return "SystemUser";
        }

        if (!user.CanLogin)
        {
            return "UserDisabled";
        }

        if (check == PasswordCheck.SuccessRehashNeeded)
        {
            user.RehashPassword(passwords.Hash(user, password));
        }

        return null;
    }

    private async Task RecordFailureAsync(string key, string account, string reason, LoginCommand command, DateTimeOffset now, CancellationToken ct)
    {
        var after = await throttles.RecordFailureAsync(key, now, ct);
        securityEvents.Write(Event("LoginFailed", SecuritySeverity.Warning, reason, account, command));
        if (after.LockedUntil is { } lockedUntil && lockedUntil > now)
        {
            securityEvents.Write(Event("AccountLocked", SecuritySeverity.High, $"Level{after.LockLevel}", account, command));
        }
    }

    // 登录时的角色：优先上次使用的，已失效或没有记录时取排在最前的启用角色
    private async Task<Role?> ChooseRoleAsync(User user, CancellationToken ct)
    {
        var granted = user.Grants.Select(g => g.RoleId).ToList();
        if (granted.Count == 0)
        {
            return null;
        }

        var active = await db.Roles.Where(r => granted.Contains(r.Id) && r.Status == RoleStatus.Active).OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToListAsync(ct);
        return active.Find(r => r.Id == user.LastRoleId) ?? active.FirstOrDefault();
    }

    private static SecurityEvent Event(string type, SecuritySeverity severity, string? reason, string account, LoginCommand command) =>
        new(type, severity, reason)
        {
            AttemptedAccount = Truncate(account, 100),
            Details = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ip"] = command.ClientIp ?? string.Empty,
                ["client"] = command.ClientType,
                ["agent"] = Truncate(command.UserAgent, 200) ?? string.Empty,
            },
        };

    private static string? Truncate(string? text, int length) => text is null || text.Length <= length ? text : text[..length];
}
