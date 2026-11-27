using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Identity.Application.SecurityEvents;

public static class SecurityEventSort
{
    /// <summary>安全事件列表排序字段表，默认按时间倒序。</summary>
    public static readonly SortMap<SecurityEventRow> Map = SortMap<SecurityEventRow>
        .Field("occurredAt", x => x.OccurredAt)
        .Field("type", x => x.Type)
        .Field("severity", x => x.Severity)
        .Default("-occurredAt");
}
