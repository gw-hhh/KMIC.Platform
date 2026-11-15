using System.ComponentModel.DataAnnotations;

namespace Kmic.Notifications.Application;

/// <summary>
/// 通知相关数据的保留期
/// </summary>
/// <remarks>
/// 配置节为 Notifications:Retention；各类记录按各自保留期由每日任务清理
/// </remarks>
public sealed class NotificationsRetentionOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Notifications:Retention";

    /// <summary>
    /// 已读站内消息的保留天数
    /// </summary>
    [Range(1, 3650)]
    public int SiteMessageReadDays { get; init; } = 90;

    /// <summary>
    /// 未读站内消息的保留天数
    /// </summary>
    [Range(1, 3650)]
    public int SiteMessageUnreadDays { get; init; } = 365;

    /// <summary>
    /// 已完成投递及发送记录的保留天数
    /// </summary>
    [Range(1, 3650)]
    public int DeliveryDays { get; init; } = 180;

    /// <summary>
    /// 捕获内容的保留天数
    /// </summary>
    [Range(1, 365)]
    public int CapturedDays { get; init; } = 30;
}
