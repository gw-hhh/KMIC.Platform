using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Notifications.Application.SiteMessages.List;

/// <summary>站内消息列表排序字段映射（默认未读在前、按发送时间倒序）</summary>
public static class SiteMessageSort
{
    public static readonly SortMap<SiteMessageItem> Map = SortMap<SiteMessageItem>
        .Field("isRead", x => x.IsRead)
        .Field("sentAt", x => x.SentAt)
        .Default("isRead,-sentAt");
}
