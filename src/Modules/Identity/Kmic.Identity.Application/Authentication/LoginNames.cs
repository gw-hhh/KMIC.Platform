using System.Security.Cryptography;
using System.Text;

namespace Kmic.Identity.Application.Authentication;

/// <summary>
/// 登录名的规范化与哈希
/// </summary>
public static class LoginNames
{
    /// <summary>
    /// 失败计数的键：规范化（去空白、转小写）后的登录名的 SHA-256，小写十六进制。不存在的登录名只存这个哈希
    /// </summary>
    public static string Hash(string account)
    {
        ArgumentNullException.ThrowIfNull(account);
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(account.Trim().ToLowerInvariant())));
    }
}
