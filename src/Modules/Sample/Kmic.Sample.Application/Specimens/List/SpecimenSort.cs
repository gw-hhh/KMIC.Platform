using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.Sample.Application.Specimens.List;

public static class SpecimenSort
{
    /// <summary>示例列表允许排序的字段，默认按创建时间倒序</summary>
    public static readonly SortMap<SpecimenListItem> Map = SortMap<SpecimenListItem>
        .Field("number", x => x.Number)
        .Field("title", x => x.Title)
        .Field("createdAt", x => x.CreatedAt)
        .Default("-createdAt");
}
