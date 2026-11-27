namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>列表端点允许排序的字段及默认排序元数据</summary>
public sealed record SortableMetadata(IReadOnlyList<string> Fields, string DefaultSort);
