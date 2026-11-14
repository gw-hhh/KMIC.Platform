using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Files.Application.Administration;

/// <summary>
/// 存储问题列表允许排序的字段
/// </summary>
public static class StorageIssueSort
{
    /// <summary>
    /// 排序字段表，默认按发现时间倒序
    /// </summary>
    public static readonly SortMap<StorageIssueItem> Map = SortMap<StorageIssueItem>
        .Field("foundAt", x => x.FoundAt)
        .Default("-foundAt");
}
