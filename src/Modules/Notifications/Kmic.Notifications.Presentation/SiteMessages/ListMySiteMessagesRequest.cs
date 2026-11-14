namespace Kmic.Notifications.Presentation.SiteMessages;

/// <summary>
/// 查询我的站内消息的参数
/// </summary>
/// <param name="Unread">已读状态筛选：true 为未读，false 为已读，未指定则返回全部</param>
/// <param name="Category">消息类别筛选</param>
/// <param name="Page">页码，从 1 开始</param>
/// <param name="PageSize">每页数量，1 到 200，默认 20</param>
/// <param name="Sort">排序，如 isRead,-sentAt：逗号分隔，字段名前加 - 表示倒序，默认未读在前，再按发送时间倒序</param>
public sealed record ListMySiteMessagesRequest(bool? Unread, string? Category, int Page = 1, int PageSize = 20, string? Sort = null);
