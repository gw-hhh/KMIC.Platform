using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Notifications.Application.SiteMessages.List;

/// <summary>
/// 站内消息列表允许排序的字段
/// </summary>
public static class SiteMessageSort
{
    /// <summary>
    /// 排序字段表，默认未读在前、新的在前
    /// </summary>
    public static readonly SortMap<SiteMessageItem> Map = SortMap<SiteMessageItem>
        .Field("isRead", x => x.IsRead)
        .Field("sentAt", x => x.SentAt)
        .Default("isRead,-sentAt");
}
