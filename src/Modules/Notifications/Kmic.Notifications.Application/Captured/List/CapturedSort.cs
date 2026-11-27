using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Notifications.Application.Captured.List;

/// <summary>捕获列表排序字段映射（默认按捕获时间倒序）</summary>
public static class CapturedSort
{
    public static readonly SortMap<CapturedListItem> Map = SortMap<CapturedListItem>
        .Field("capturedAt", x => x.CapturedAt)
        .Default("-capturedAt");
}
