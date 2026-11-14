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
    /// 高优先级，如验证码通知，由投递工作器优先领取
    /// </summary>
    High,
}
