namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>排序参数，格式如 -createdAt,number（逗号分隔，字段名前加 - 表示降序）。</summary>
/// <param name="Sort">排序表达式</param>
public sealed record SortRequest(string? Sort = null)
{
    /// <summary>允许同时排序的最大字段数。</summary>
    public const int MaxFields = 3;
}
