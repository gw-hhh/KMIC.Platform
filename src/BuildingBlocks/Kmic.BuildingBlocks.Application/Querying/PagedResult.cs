namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 一页数据
/// </summary>
/// <param name="Items">本页的数据，页码超过末页时为空</param>
/// <param name="Page">页码</param>
/// <param name="PageSize">每页数量</param>
/// <param name="TotalCount">总数；上限计数且超过上限时等于上限</param>
/// <param name="TotalCountExact">总数是否精确；false 表示实际数量超过 TotalCount，前端显示“10,000+”</param>
/// <typeparam name="T">列表项类型</typeparam>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long TotalCount, bool TotalCountExact);
