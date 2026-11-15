using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 生成管理员重置密码时使用的临时密码
/// </summary>
/// <remarks>
/// 排除易混淆字符（0、O、1、l、I），便于人工传递。
/// 临时密码仅在重置响应中返回，不写日志或审计，不通过短信或邮件发送；登录后必须修改密码
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
    /// 使用密码学随机数生成临时密码，长度不低于配置的最小值
    /// </summary>
    public string Generate() => new(RandomNumberGenerator.GetItems<char>(_alphabet, Math.Max(PreferredLength, settings.Value.MinLength)));
}
