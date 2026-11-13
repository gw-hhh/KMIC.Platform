using System.ComponentModel.DataAnnotations;

namespace Kmic.Notifications.Infrastructure.Channels;

/// <summary>
/// 一个发件账号：发件人身份和 SMTP 服务器
/// </summary>
public sealed class EmailSenderOptions
{
    /// <summary>
    /// 发件人地址
    /// </summary>
    [Required]
    public string Address { get; init; } = string.Empty;

    /// <summary>
    /// 发件人显示名
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// SMTP 服务器地址
    /// </summary>
    [Required]
    public string Host { get; init; } = string.Empty;

    /// <summary>
    /// SMTP 端口
    /// </summary>
    [Range(1, 65535)]
    public int Port { get; init; } = 587;

    /// <summary>
    /// 连接的加密方式：StartTls（先明文再升级）、SslOnConnect（一开始就加密）、None（不加密，只在内网的测试服务器上用）
    /// </summary>
    [Required]
    public string Security { get; init; } = "StartTls";

    /// <summary>
    /// SMTP 账号，不需要认证的服务器留空
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// SMTP 密码，来自密钥源，不能写在配置文件里
    /// </summary>
    public string Password { get; init; } = string.Empty;
}
