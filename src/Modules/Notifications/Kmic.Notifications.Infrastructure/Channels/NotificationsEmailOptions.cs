using System.ComponentModel.DataAnnotations;

namespace Kmic.Notifications.Infrastructure.Channels;

/// <summary>
/// 邮件渠道部署配置，默认发件账号名为 Default
/// </summary>
/// <remarks>
/// 节名 Notifications:Email。模板声明用哪个发件账号，没声明就用 Default。密码来自密钥源，不写在配置文件里
/// </remarks>
public sealed class NotificationsEmailOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Notifications:Email";

    /// <summary>
    /// 发件账号，键是账号名
    /// </summary>
    public IDictionary<string, EmailSenderOptions> Senders { get; init; } = new Dictionary<string, EmailSenderOptions>(StringComparer.Ordinal);

    /// <summary>
    /// SMTP 操作超时阈值（秒）
    /// </summary>
    [Range(5, 300)]
    public int TimeoutSeconds { get; init; } = 30;
}
