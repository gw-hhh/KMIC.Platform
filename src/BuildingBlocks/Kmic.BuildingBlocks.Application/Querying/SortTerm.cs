namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 排序表达式中的一项
/// </summary>
internal sealed record SortTerm(string Name, bool Descending);
