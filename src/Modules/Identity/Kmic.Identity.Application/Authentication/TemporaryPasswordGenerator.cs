using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 生成管理员重置密码用的临时密码，不含 0、O、1、l、I 这类易混淆字符
/// </summary>
/// <remarks>
/// 临时密码只在重置响应里返回，不写日志和审计，也不经短信或邮件发送
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
