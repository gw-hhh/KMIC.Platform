using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Identity.Application.SecurityEvents;

/// <summary>
/// 安全事件列表允许排序的字段
/// </summary>
public static class SecurityEventSort
{
    /// <summary>
    /// 排序字段表，默认按时间倒序
    /// </summary>
    public static readonly SortMap<SecurityEventRow> Map = SortMap<SecurityEventRow>
        .Field("occurredAt", x => x.OccurredAt)
        .Field("type", x => x.Type)
        .Field("severity", x => x.Severity)
        .Default("-occurredAt");
}
