using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Authentication.ChangePassword;

/// <summary>
/// 修改当前用户密码
/// </summary>
/// <remarks>
/// 旧密码输错和登录失败共用一份失败计数，猜密码的总次数有上限
/// </remarks>
public sealed class ChangePasswordCommandHandler(
    IUserRepository users,
    IExecutionContext execution,
    IPasswordService passwords,
    ILoginThrottleStore throttles,
    PasswordPolicy policy,
    ISecurityEventWriter securityEvents,
    TimeProvider time) : ICommandHandler<ChangePasswordCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var access = execution.Access ?? throw new InvalidOperationException("修改密码需要已登录的访问上下文。");
        var loaded = await users.GetForUpdateAsync(access.UserId, null, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var user = loaded.Value;
        var now = time.GetUtcNow();
        var key = LoginNames.Hash(user.Account);
        if (!string.IsNullOrEmpty(user.PasswordHash))
        {
            if (LoginThrottlePolicy.Remaining(await throttles.GetAsync(key, cancellationToken), now) is { } wait)
            {
                return IdentityErrors.LoginThrottled.WithMetadata(ErrorMetadataKeys.RetryAfterSeconds, (int)Math.Ceiling(wait.TotalSeconds));
            }

            if (command.OldPassword is null || passwords.Verify(user, command.OldPassword) == PasswordCheck.Failed)
            {
                var after = await throttles.RecordFailureAsync(key, now, cancellationToken);
                securityEvents.Write(new SecurityEvent("PasswordChangeFailed", SecuritySeverity.Warning, "OldPasswordWrong"));
                if (after.LockedUntil is { } lockedUntil && lockedUntil > now)
                {
                    securityEvents.Write(new SecurityEvent("AccountLocked", SecuritySeverity.High, $"Level{after.LockLevel}") { UserId = user.Id.Value });
                }

                return IdentityErrors.OldPasswordWrong;
            }

            await throttles.ClearAsync(key, cancellationToken);
        }

        var allowed = policy.Validate(command.NewPassword, user.Account, user.Mobile);
        if (allowed.IsFailure)
        {
            return allowed;
        }

        var set = user.SetPassword(passwords.Hash(user, command.NewPassword), mustChange: false, byAdministrator: false, now);
        if (set.IsSuccess)
        {
            securityEvents.Write(new SecurityEvent("PasswordChanged", SecuritySeverity.Info, null));
        }

        return set;
    }
}
