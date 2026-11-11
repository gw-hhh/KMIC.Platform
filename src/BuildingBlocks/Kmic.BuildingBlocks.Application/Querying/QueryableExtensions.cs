using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 查询扩展
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// 不按数据权限过滤
    /// </summary>
    /// <remarks>
    /// 软删除过滤仍然生效。处理器要标 [BypassesDataScope] 写明原因，如编号要在全部数据中判断是否重复
    /// </remarks>
    public static IQueryable<T> WithoutDataScope<T>(this IQueryable<T> query)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.IgnoreQueryFilters([QueryFilters.DataScope]);
    }
}
