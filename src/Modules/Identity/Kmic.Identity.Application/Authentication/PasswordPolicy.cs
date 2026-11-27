using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Authentication;

public sealed class PasswordPolicy(IOptions<PasswordOptions> settings, IWeakPasswordList weakPasswords)
{
    private PasswordOptions Options => settings.Value;

    /// <summary>校验新密码：须满足长度要求、非弱密码，且不与账号或手机号相同</summary>
    /// <param name="mobile">手机号，未绑定时为空</param>
    public Result Validate(string? password, string account, string? mobile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(account);

        if (string.IsNullOrEmpty(password) || password.Length < Options.MinLength || password.Length > Options.MaxLength)
        {
            return IdentityErrors.PasswordTooWeak.WithDetail($"密码长度须为 {Options.MinLength} 到 {Options.MaxLength} 个字符");
        }

        if (weakPasswords.Contains(password) || SameAs(password, account) || (mobile is not null && SameAs(password, mobile)))
        {
            return IdentityErrors.PasswordTooWeak;
        }

        return Result.Success();
    }

    // 同时拒绝带 +86 前缀和不带前缀的手机号作为密码
    private static bool SameAs(string password, string other)
    {
        if (string.Equals(password, other, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return other.StartsWith("+86", StringComparison.Ordinal) && string.Equals(password, other[3..], StringComparison.Ordinal);
    }
}
