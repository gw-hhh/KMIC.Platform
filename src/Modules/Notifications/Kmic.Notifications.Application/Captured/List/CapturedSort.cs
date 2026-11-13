using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Notifications.Application.Captured.List;

/// <summary>
/// 捕获列表允许排序的字段
/// </summary>
public static class CapturedSort
{
    /// <summary>
    /// 排序字段表，默认新的在前
    /// </summary>
    public static readonly SortMap<CapturedListItem> Map = SortMap<CapturedListItem>
        .Field("capturedAt", x => x.CapturedAt)
        .Default("-capturedAt");
}
