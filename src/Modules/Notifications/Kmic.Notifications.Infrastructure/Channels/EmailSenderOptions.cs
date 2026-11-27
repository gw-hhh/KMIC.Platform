using System.ComponentModel.DataAnnotations;

namespace Kmic.Notifications.Infrastructure.Channels;

/// <summary>发件账号配置（发件人身份及 SMTP 服务器）</summary>
public sealed class EmailSenderOptions
{
    /// <summary>发件人地址</summary>
    [Required]
    public string Address { get; init; } = string.Empty;

    /// <summary>发件人显示名</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>SMTP 服务器地址</summary>
    [Required]
    public string Host { get; init; } = string.Empty;

    /// <summary>SMTP 端口</summary>
    [Range(1, 65535)]
    public int Port { get; init; } = 587;

    /// <summary>连接加密方式：StartTls、SslOnConnect 或 None（不加密，仅用于内网测试）</summary>
    [Required]
    public string Security { get; init; } = "StartTls";

    /// <summary>SMTP 账号（无需认证时留空）</summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>SMTP 密码（须从密钥源读取，严禁写入配置文件）</summary>
    public string Password { get; init; } = string.Empty;
}
