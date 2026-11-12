namespace Kmic.BuildingBlocks.Contracts.Notifications;

/// <summary>
/// 通知渠道
/// </summary>
[Flags]
public enum NotificationChannels
{
    /// <summary>
    /// 没有渠道
    /// </summary>
    None = 0,

    /// <summary>
    /// 邮件
    /// </summary>
    Email = 1,

    /// <summary>
    /// 短信
    /// </summary>
    Sms = 2,

    /// <summary>
    /// 站内消息
    /// </summary>
    InApp = 4,
}
