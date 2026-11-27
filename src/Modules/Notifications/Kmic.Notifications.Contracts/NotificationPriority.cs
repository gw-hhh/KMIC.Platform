namespace Kmic.Notifications.Contracts;

/// <summary>通知优先级</summary>
public enum NotificationPriority
{
    /// <summary>普通</summary>
    Normal,

    /// <summary>高优先级（如验证码），投递任务会优先领取</summary>
    High,
}
