namespace Kmic.Notifications.Presentation.SiteMessages;

/// <summary>
/// 查询我的站内消息的参数
/// </summary>
/// <param name="Unread">是否未读，不传时返回全部</param>
/// <param name="Category">消息类别筛选</param>
/// <param name="Page">页码</param>
/// <param name="PageSize">每页数量</param>
/// <param name="Sort">排序</param>
public sealed record ListMySiteMessagesRequest(bool? Unread, string? Category, int Page = 1, int PageSize = 20, string? Sort = null);
