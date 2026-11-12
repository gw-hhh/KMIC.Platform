using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Authentication.ChangePassword;

/// <summary>
/// 处理 <see cref="ChangePasswordCommand"/>
/// </summary>
public sealed class ChangePasswordCommandHandler(
    IUserRepository users,
    IExecutionContext execution,
    IPasswordService passwords,
    PasswordPolicy policy,
    ISecurityEventWriter securityEvents,
    TimeProvider time) : ICommandHandler<ChangePasswordCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var access = execution.Access ?? throw new InvalidOperationException("修改密码需要已登录的访问上下文。");
        var loaded = await users.GetForUpdateAsync(access.UserId, null, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var user = loaded.Value;
        if (!string.IsNullOrEmpty(user.PasswordHash) && (command.OldPassword is null || passwords.Verify(user, command.OldPassword) == PasswordCheck.Failed))
        {
            return IdentityErrors.OldPasswordWrong;
        }

        var allowed = policy.Validate(command.NewPassword, user.Account, user.Mobile);
        if (allowed.IsFailure)
        {
            return allowed;
        }

        var set = user.SetPassword(passwords.Hash(user, command.NewPassword), mustChange: false, byAdministrator: false, time.GetUtcNow());
        if (set.IsSuccess)
        {
            securityEvents.Write(new SecurityEvent("PasswordChanged", SecuritySeverity.Info, null));
        }

        return set;
    }
}
