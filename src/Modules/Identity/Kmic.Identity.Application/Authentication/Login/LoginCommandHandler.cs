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
    public async ValueTask<Result<AuthTokens>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow();
        var account = command.Account.Trim();
        var key = LoginNames.Hash(account);

        // 先检查限流状态再查询账号，确保无论账号是否存在响应行为一致
        var state = await throttles.GetAsync(key, cancellationToken);
        if (LoginThrottlePolicy.Remaining(state, now) is { } wait)
        {
            securityEvents.Write(Event("LoginFailed", SecuritySeverity.Warning, "Throttled", account, command));
            return IdentityErrors.LoginThrottled.WithMetadata(ErrorMetadataKeys.RetryAfterSeconds, (int)Math.Ceiling(wait.TotalSeconds));
        }

        var user = await users.FindByAccountAsync(account, cancellationToken);
        var failure = CheckCredentials(user, command.Password);
        if (failure is { } reason)
        {
            await RecordFailureAsync(key, account, reason, command, now, cancellationToken);
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

    // 校验成功返回 null，并在需要时升级密码哈希
    private LoginFailure? CheckCredentials(User? user, string password)
    {
        if (user is null)
        {
            // 账号不存在时仍执行等效哈希校验，使响应耗时与密码错误相近
            passwords.Burn(password);
            return LoginFailure.UnknownAccount;
        }

        var check = passwords.Verify(user, password);
        if (check == PasswordCheck.Failed)
        {
            return LoginFailure.WrongPassword;
        }

        if (user.Kind == UserKind.System)
        {
            return LoginFailure.SystemUser;
        }

        if (!user.CanLogin)
        {
            return LoginFailure.UserDisabled;
        }

        if (check == PasswordCheck.SuccessRehashNeeded)
        {
            user.RehashPassword(passwords.Hash(user, password));
        }

        return null;
    }

    private async Task RecordFailureAsync(string key, string account, LoginFailure reason, LoginCommand command, DateTimeOffset now, CancellationToken ct)
    {
        var after = await throttles.RecordFailureAsync(key, now, ct);
        securityEvents.Write(Event("LoginFailed", SecuritySeverity.Warning, reason.ToString(), account, command));
        if (after.LockedUntil is { } lockedUntil && lockedUntil > now)
        {
            securityEvents.Write(Event("AccountLocked", SecuritySeverity.High, $"Level{after.LockLevel}", account, command));
        }
    }

    // 登录默认角色：优先沿用上次使用的角色，失效或无记录时取排序最前的启用角色
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

    // 登录失败原因枚举名将原样写入安全事件，禁止重命名
    private enum LoginFailure
    {
        UnknownAccount,
        WrongPassword,
        SystemUser,
        UserDisabled,
    }

    private static string? Truncate(string? text, int length) => text is null || text.Length <= length ? text : text[..length];
}
