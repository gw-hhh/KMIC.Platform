using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Notifications.Application.SiteMessages.List;

/// <summary>
/// 查询我的站内消息：只有自己的，未读的在前、新的在前
/// </summary>
/// <param name="Unread">只看未读（true）或只看已读（false），不给就都看</param>
/// <param name="Category">只看某个类别</param>
/// <param name="Page">分页</param>
/// <param name="Sort">排序</param>
[AllowAuthenticated]
public sealed record ListMySiteMessagesQuery(bool? Unread, string? Category, PageRequest Page, SortRequest Sort) : IQuery<PagedResult<SiteMessageItem>>;
