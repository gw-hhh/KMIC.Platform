using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 新密码要满足的规则
/// </summary>
/// <param name="settings">长度规则</param>
/// <param name="weakPasswords">常见弱密码清单</param>
public sealed class PasswordPolicy(IOptions<PasswordOptions> settings, IWeakPasswordList weakPasswords)
{
    private PasswordOptions Options => settings.Value;

    /// <summary>
    /// 校验新密码：长度在范围内、不是常见弱密码、不与账号或手机号相同
    /// </summary>
    /// <param name="password">新密码</param>
    /// <param name="account">用户的账号</param>
    /// <param name="mobile">用户的手机号，没有时为空</param>
    public Result Validate(string? password, string account, string? mobile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(account);

        if (string.IsNullOrEmpty(password) || password.Length < Options.MinLength || password.Length > Options.MaxLength)
        {
            return IdentityErrors.PasswordTooWeak.WithDetail($"密码长度要在 {Options.MinLength} 到 {Options.MaxLength} 个字符之间");
        }

        if (weakPasswords.Contains(password) || SameAs(password, account) || (mobile is not null && SameAs(password, mobile)))
        {
            return IdentityErrors.PasswordTooWeak;
        }

        return Result.Success();
    }

    // 手机号带了 +86 前缀时，用户输入的往往是不带前缀的 11 位，两种写法都要挡
    private static bool SameAs(string password, string other)
    {
        if (string.Equals(password, other, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return other.StartsWith("+86", StringComparison.Ordinal) && string.Equals(password, other[3..], StringComparison.Ordinal);
    }
}
