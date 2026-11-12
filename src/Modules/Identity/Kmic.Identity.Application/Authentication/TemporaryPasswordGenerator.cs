using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 生成管理员重置密码时给用户的一次性临时密码
/// </summary>
/// <remarks>
/// 临时密码要靠人念出来或抄下来交给用户，所以字母表里去掉了容易看错的字符（0 与 O、1 与 l 与 I）。
/// 用户用它登录后马上要改密码，它只在重置的那次响应里出现，不写日志、不进审计、不经短信或邮件发送
/// </remarks>
public sealed class TemporaryPasswordGenerator(IOptions<PasswordOptions> settings)
{
    private const string Confusable = "0O1lI";

    private const int PreferredLength = 14;

    private static readonly char[] _alphabet =
    [
        .. Enumerable.Range(0, 128).Select(code => (char)code).Where(c => char.IsAsciiLetterOrDigit(c) && !Confusable.Contains(c, StringComparison.Ordinal)),
    ];

    /// <summary>
    /// 生成一个临时密码：用密码学随机数，长度不少于密码策略要求的最小长度
    /// </summary>
    public string Generate() => new(RandomNumberGenerator.GetItems<char>(_alphabet, Math.Max(PreferredLength, settings.Value.MinLength)));
}
