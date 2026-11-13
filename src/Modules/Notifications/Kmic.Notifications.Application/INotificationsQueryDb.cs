using Kmic.Notifications.Domain.SiteMessages;

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
}
