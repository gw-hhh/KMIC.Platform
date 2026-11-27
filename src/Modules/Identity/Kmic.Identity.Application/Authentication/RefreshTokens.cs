using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace Kmic.Identity.Application.Authentication;

public static class RefreshTokens
{
    private const int TokenBytes = 32;

    /// <summary>生成 32 字节随机刷新令牌（Base64Url 编码）</summary>
    public static string Generate() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenBytes));

    /// <summary>计算令牌的 SHA-256 哈希（小写十六进制），仅持久化哈希值</summary>
    public static string Hash(string token)
    {
        ArgumentNullException.ThrowIfNull(token);
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
