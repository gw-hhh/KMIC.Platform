namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>分页查询结果；页码超出末页时 <paramref name="Items"/> 为空。</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount, bool TotalCountExact);
