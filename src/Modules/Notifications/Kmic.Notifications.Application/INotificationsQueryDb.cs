using Kmic.Notifications.Domain.Captured;
using Kmic.Notifications.Domain.Deliveries;
using Kmic.Notifications.Domain.SiteMessages;
using Kmic.Notifications.Domain.Templates;

namespace Kmic.Notifications.Application;

/// <summary>
/// Notifications 模块的查询入口
/// </summary>
/// <remarks>
/// 查询处理器只从它出发：已经带了数据范围和软删除过滤，也不会读到命令里还没保存的修改。
/// 站内消息不按组织范围，处理器要自己固定收件人条件
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
    /// 投递记录；尝试次数、失败原因这些影子列用 EF.Property 读取，列名见 DeliveryColumns
    /// </summary>
    IQueryable<Delivery> Deliveries { get; }

    /// <summary>
    /// 非生产环境捕获的内容
    /// </summary>
    IQueryable<CapturedMessage> CapturedMessages { get; }
}
