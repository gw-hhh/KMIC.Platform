using System.Security.Cryptography;
using System.Text;

namespace Kmic.Identity.Application.Authentication;

public static class LoginNames
{
    /// <summary>生成登录失败计数键：去首尾空白并转小写后计算 SHA-256（小写十六进制）</summary>
    public static string Hash(string account)
    {
        ArgumentNullException.ThrowIfNull(account);
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(account.Trim().ToLowerInvariant())));
    }
}
