using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Notifications.Application.Deliveries.List;

/// <summary>
/// 投递列表允许排序的字段
/// </summary>
public static class DeliverySort
{
    /// <summary>
    /// 排序字段表，默认新的在前
    /// </summary>
    public static readonly SortMap<DeliveryListItem> Map = SortMap<DeliveryListItem>
        .Field("createdAt", x => x.CreatedAt)
        .Field("sentAt", x => x.SentAt)
        .Field("attempts", x => x.Attempts)
        .Field("templateKey", x => x.TemplateKey)
        .Default("-createdAt");
}
