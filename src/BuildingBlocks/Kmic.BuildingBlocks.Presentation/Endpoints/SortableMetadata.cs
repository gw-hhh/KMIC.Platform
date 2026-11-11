namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 列表端点允许排序的字段，由 <c>Sortable(...)</c> 写入端点元数据
/// </summary>
/// <param name="Fields">可排序的字段名</param>
/// <param name="DefaultSort">默认排序</param>
public sealed record SortableMetadata(IReadOnlyList<string> Fields, string DefaultSort);
