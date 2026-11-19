using Kmic.Notifications.Domain.Captured;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.SiteMessages;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application;

/// <summary>
/// Notifications 模块的查询入口
/// </summary>
/// <remarks>
/// 站内消息要由处理器限定收件人
/// </remarks>
public interface INotificationsQueryDb
{
    /// <summary>
    /// 站内消息
    /// </summary>
    IQueryable<SiteMessage> SiteMessages { get; }

    /// <summary>
    /// 通知模板
    /// </summary>
    IQueryable<NotificationTemplate> Templates { get; }

    /// <summary>
    /// 模板的发布版本
    /// </summary>
    IQueryable<TemplateVersion> TemplateVersions { get; }

    /// <summary>
    /// 投递记录
    /// </summary>
    /// <remarks>
    /// 影子属性用 EF.Property 读，列名见 DeliveryColumns
    /// </remarks>
    IQueryable<Delivery> Deliveries { get; }

    /// <summary>
    /// 捕获的通知内容
    /// </summary>
    IQueryable<CapturedMessage> CapturedMessages { get; }
}
