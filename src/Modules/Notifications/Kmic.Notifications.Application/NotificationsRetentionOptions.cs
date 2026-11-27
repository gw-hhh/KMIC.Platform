using System.ComponentModel.DataAnnotations;

namespace Kmic.Notifications.Application;

/// <summary>通知数据保留期配置</summary>
public sealed class NotificationsRetentionOptions
{
    public const string Section = "Notifications:Retention";

    /// <summary>已读站内消息保留天数</summary>
    [Range(1, 3650)]
    public int SiteMessageReadDays { get; init; } = 90;

    /// <summary>未读站内消息保留天数</summary>
    [Range(1, 3650)]
    public int SiteMessageUnreadDays { get; init; } = 365;

    /// <summary>投递及发送记录保留天数</summary>
    [Range(1, 3650)]
    public int DeliveryDays { get; init; } = 180;

    /// <summary>捕获内容保留天数</summary>
    [Range(1, 365)]
    public int CapturedDays { get; init; } = 30;
}
