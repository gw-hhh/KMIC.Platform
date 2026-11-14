using System.Security.Cryptography;
using System.Text;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 登录名的规范化与哈希
/// </summary>
public static class LoginNames
{
    /// <summary>
    /// 登录失败计数键：去除首尾空白、转小写后计算 SHA-256，返回小写十六进制
    /// </summary>
    public static string Hash(string account)
    {
        ArgumentNullException.ThrowIfNull(account);
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(account.Trim().ToLowerInvariant())));
    }
}
