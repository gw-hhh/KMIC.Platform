namespace Kmic.Notifications.Contracts;

/// <summary>
/// 通知的优先级
/// </summary>
public enum NotificationPriority
{
    /// <summary>
    /// 普通
    /// </summary>
    Normal,

    /// <summary>
    /// 高：验证码这类要立刻送到的通知，投递程序优先领取
    /// </summary>
    High,
}
