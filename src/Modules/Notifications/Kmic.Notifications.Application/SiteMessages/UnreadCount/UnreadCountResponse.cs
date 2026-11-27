namespace Kmic.Notifications.Application.SiteMessages.UnreadCount;

/// <summary>未读站内消息数</summary>
/// <param name="Count">条数</param>
public sealed record UnreadCountResponse(int Count);
