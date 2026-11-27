using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Notifications.Application.Deliveries.List;

/// <summary>投递列表排序字段映射（默认按创建时间倒序）</summary>
public static class DeliverySort
{
    public static readonly SortMap<DeliveryListItem> Map = SortMap<DeliveryListItem>
        .Field("createdAt", x => x.CreatedAt)
        .Field("sentAt", x => x.SentAt)
        .Field("attempts", x => x.Attempts)
        .Field("templateKey", x => x.TemplateKey)
        .Default("-createdAt");
}
