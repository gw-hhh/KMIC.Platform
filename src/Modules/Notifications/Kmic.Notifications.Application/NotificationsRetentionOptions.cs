using System.ComponentModel.DataAnnotations;

namespace Kmic.Notifications.Application;

/// <summary>
/// 通知相关数据的保留期
/// </summary>
/// <remarks>
/// 部署配置，节名 Notifications:Retention。超过保留期的由每日的清理任务删除
/// </remarks>
public sealed class NotificationsRetentionOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Notifications:Retention";

    /// <summary>
    /// 已读的站内消息保留多少天
    /// </summary>
    [Range(1, 3650)]
    public int SiteMessageReadDays { get; init; } = 90;

    /// <summary>
    /// 一直没读的站内消息保留多少天
    /// </summary>
    [Range(1, 3650)]
    public int SiteMessageUnreadDays { get; init; } = 365;

    /// <summary>
    /// 已完成的投递与发送记录保留多少天
    /// </summary>
    [Range(1, 3650)]
    public int DeliveryDays { get; init; } = 180;

    /// <summary>
    /// 非生产环境的捕获内容保留多少天
    /// </summary>
    [Range(1, 365)]
    public int CapturedDays { get; init; } = 30;
}
