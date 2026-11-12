using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Sample.Application.Specimens.List;

/// <summary>
/// 示例标本列表允许排序的字段
/// </summary>
public static class SpecimenSort
{
    /// <summary>
    /// 排序字段表，默认按创建时间倒序
    /// </summary>
    public static readonly SortMap<SpecimenListItem> Map = SortMap<SpecimenListItem>
        .Field("number", x => x.Number)
        .Field("title", x => x.Title)
        .Field("createdAt", x => x.CreatedAt)
        .Default("-createdAt");
}
