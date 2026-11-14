using System.Security.Cryptography;
using System.Text;

namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 联系方式的脱敏与去重键
/// </summary>
internal static class AddressMasking
{
    /// <summary>
    /// 保留手机号的前 3 个和后 4 个字符，中间用星号替换；不超过 7 个字符时全部隐藏
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
    /// 地址去重键：去除首尾空白、转小写后计算 SHA-256，以 a: 开头
    /// </summary>
    public static string KeyOfAddress(string address) =>
        "a:" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(address.Trim().ToLowerInvariant())));

    /// <summary>
    /// 用户的去重键
    /// </summary>
    public static string KeyOfUser(Guid userId) => "u:" + userId.ToString("D");
}
