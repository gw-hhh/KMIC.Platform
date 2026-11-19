namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 排序参数
/// </summary>
/// <remarks>
/// 写法如 -createdAt,number：逗号分隔，字段名前加 - 表示倒序
/// </remarks>
/// <param name="Sort">排序表达式</param>
public sealed record SortRequest(string? Sort = null)
{
    /// <summary>
    /// 最多按几个字段排序
    /// </summary>
    public const int MaxFields = 3;
}
