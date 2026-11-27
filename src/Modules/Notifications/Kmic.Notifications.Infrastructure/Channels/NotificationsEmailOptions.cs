using System.ComponentModel.DataAnnotations;

namespace Kmic.Notifications.Infrastructure.Channels;

/// <summary>邮件渠道配置（默认发件账号名为 Default，密码须从密钥源读取）</summary>
public sealed class NotificationsEmailOptions
{
    public const string Section = "Notifications:Email";

    /// <summary>发件账号集合（键为账号名）</summary>
    public IDictionary<string, EmailSenderOptions> Senders { get; init; } = new Dictionary<string, EmailSenderOptions>(StringComparer.Ordinal);

    /// <summary>SMTP 操作超时时间（秒）</summary>
    [Range(5, 300)]
    public int TimeoutSeconds { get; init; } = 30;
}
