using System.Security.Cryptography;
using System.Text;

namespace Kmic.Notifications.Application.Processing;

internal static class AddressMasking
{
    // 保留前 3 位与后 4 位；不足 8 位时全部脱敏
    public static string Phone(string phone)
    {
        var digits = phone.Trim();
        return digits.Length <= 7 ? new string('*', digits.Length) : digits[..3] + new string('*', digits.Length - 7) + digits[^4..];
    }

    // 仅保留 @ 前首字符与域名，如 a***@example.com
    public static string Email(string email)
    {
        var address = email.Trim();
        var at = address.LastIndexOf('@');
        return at <= 0 ? new string('*', address.Length) : address[0] + "***" + address[at..];
    }

    // 去重键统一规范为去空白小写后的 SHA-256，前缀 a:
    public static string KeyOfAddress(string address) =>
        "a:" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(address.Trim().ToLowerInvariant())));

    public static string KeyOfUser(Guid userId) => "u:" + userId.ToString("D");
}
