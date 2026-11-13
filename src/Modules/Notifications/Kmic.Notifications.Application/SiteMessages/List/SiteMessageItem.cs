using Kmic.Notifications.Domain.SiteMessages;

namespace Kmic.Notifications.Application.SiteMessages.List;

/// <summary>
/// 站内消息列表里的一条
/// </summary>
/// <param name="Id">消息</param>
/// <param name="Title">标题</param>
/// <param name="Body">正文，前端显示时转义</param>
/// <param name="Link">点击跳转的站内地址</param>
/// <param name="Category">类别</param>
/// <param name="SentAt">发出的时间</param>
/// <param name="IsRead">是否已读</param>
/// <param name="ReadAt">已读的时间</param>
public sealed record SiteMessageItem(
    SiteMessageId Id,
    string Title,
    string Body,
    string? Link,
    string Category,
    DateTimeOffset SentAt,
    bool IsRead,
    DateTimeOffset? ReadAt);
