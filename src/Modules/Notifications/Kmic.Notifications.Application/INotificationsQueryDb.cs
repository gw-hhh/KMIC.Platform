using Kmic.Notifications.Domain.Captured;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.SiteMessages;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application;

/// <summary>Notifications 模块只读查询入口（站内消息须由处理器按收件人过滤）</summary>
public interface INotificationsQueryDb
{
    /// <summary>站内消息</summary>
    IQueryable<SiteMessage> SiteMessages { get; }

    /// <summary>通知模板</summary>
    IQueryable<NotificationTemplate> Templates { get; }

    /// <summary>模板发布版本</summary>
    IQueryable<TemplateVersion> TemplateVersions { get; }

    /// <summary>投递记录（影子属性通过 EF.Property 读取，列名见 DeliveryColumns）</summary>
    IQueryable<Delivery> Deliveries { get; }

    /// <summary>捕获的通知内容</summary>
    IQueryable<CapturedMessage> CapturedMessages { get; }
}
