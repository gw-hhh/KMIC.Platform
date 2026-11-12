using System.Security.Cryptography;
using System.Text;
using System.Buffers.Text;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 刷新令牌的生成与哈希
/// </summary>
public static class RefreshTokens
{
    private const int TokenBytes = 32;

    /// <summary>
    /// 生成新令牌：32 字节随机值，Base64Url 编码
    /// </summary>
    public static string Generate() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenBytes));

    /// <summary>
    /// 令牌的 SHA-256，小写十六进制；数据库里只存它
    /// </summary>
    public static string Hash(string token)
    {
        ArgumentNullException.ThrowIfNull(token);
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
