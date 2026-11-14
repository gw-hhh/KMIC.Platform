using Kmic.Notifications.Domain.Captured;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.SiteMessages;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application;

/// <summary>
/// Notifications 模块的查询入口
/// </summary>
/// <remarks>
/// 查询处理器通过此接口读取，不跟踪实体，也不读取命令中尚未保存的修改。
/// 通知管理按权限点控制；站内消息须由处理器限定收件人
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
    /// 领取次数、失败原因等影子属性通过 EF.Property 读取，名称见 DeliveryColumns
    /// </remarks>
    IQueryable<Delivery> Deliveries { get; }

    /// <summary>
    /// 捕获的通知内容
    /// </summary>
    IQueryable<CapturedMessage> CapturedMessages { get; }
}
