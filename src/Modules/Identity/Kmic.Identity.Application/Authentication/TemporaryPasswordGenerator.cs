using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Application.Authentication;

/// <summary>生成临时密码（排除易混淆字符，仅通过重置响应返回，禁止写入日志）</summary>
public sealed class TemporaryPasswordGenerator(IOptions<PasswordOptions> settings)
{
    private const string Confusable = "0O1lI";

    private const int PreferredLength = 14;

    private static readonly char[] _alphabet =
    [
        .. Enumerable.Range(0, 128).Select(code => (char)code).Where(c => char.IsAsciiLetterOrDigit(c) && !Confusable.Contains(c, StringComparison.Ordinal)),
    ];

    /// <summary>生成加密随机临时密码，长度不低于配置最小值</summary>
    public string Generate() => new(RandomNumberGenerator.GetItems<char>(_alphabet, Math.Max(PreferredLength, settings.Value.MinLength)));
}
