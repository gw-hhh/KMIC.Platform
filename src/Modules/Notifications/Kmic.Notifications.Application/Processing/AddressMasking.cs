using System.Security.Cryptography;
using System.Text;

namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 联系方式的脱敏与去重键
/// </summary>
internal static class AddressMasking
{
    /// <summary>
    /// 手机号只留前三位和后四位，如 138****0000
    /// </summary>
    public static string Phone(string phone)
    {
        var digits = phone.Trim();
        return digits.Length <= 7 ? new string('*', digits.Length) : digits[..3] + new string('*', digits.Length - 7) + digits[^4..];
    }

    /// <summary>
    /// 邮箱只留本地部分的第一个字符和域名，如 a***@example.com
    /// </summary>
    public static string Email(string email)
    {
        var address = email.Trim();
        var at = address.LastIndexOf('@');
        return at <= 0 ? new string('*', address.Length) : address[0] + "***" + address[at..];
    }

    /// <summary>
    /// 直接给的地址的去重键：地址小写的 SHA-256，不把地址本身放进键里
    /// </summary>
    public static string KeyOfAddress(string address) =>
        "a:" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(address.Trim().ToLowerInvariant())));

    /// <summary>
    /// 用户的去重键
    /// </summary>
    public static string KeyOfUser(Guid userId) => "u:" + userId.ToString("D");
}
