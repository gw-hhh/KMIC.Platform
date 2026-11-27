namespace Kmic.Notifications.Application.SiteMessages.MarkRead;

/// <summary>标为已读结果</summary>
/// <param name="Count">本次标记为已读的消息数</param>
public sealed record MarkedReadResponse(int Count);
