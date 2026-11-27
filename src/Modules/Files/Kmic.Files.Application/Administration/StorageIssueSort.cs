using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Files.Application.Administration;

public static class StorageIssueSort
{
    /// <summary>默认按发现时间倒序</summary>
    public static readonly SortMap<StorageIssueItem> Map = SortMap<StorageIssueItem>
        .Field("foundAt", x => x.FoundAt)
        .Default("-foundAt");
}
