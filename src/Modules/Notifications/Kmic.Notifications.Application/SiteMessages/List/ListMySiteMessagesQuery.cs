using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.SiteMessages.List;

/// <summary>
/// 查询本人的站内消息，默认未读在前、发送时间倒序
/// </summary>
/// <param name="Unread">是否未读：true 为未读，false 为已读；不传时返回全部</param>
/// <param name="Category">消息类别筛选</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[AllowAuthenticated]
public sealed record ListMySiteMessagesQuery(bool? Unread, string? Category, PageRequest Page, SortRequest Sort) : IQuery<PagedResult<SiteMessageItem>>;
