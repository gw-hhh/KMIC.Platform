namespace Kmic.Notifications.Application.SiteMessages.MarkRead;

/// <summary>
/// 标为已读的结果
/// </summary>
/// <param name="Count">这次标了几条</param>
public sealed record MarkedReadResponse(int Count);
