using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 查询扩展
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// 不按数据权限过滤，软删除过滤仍然生效
    /// </summary>
    /// <remarks>
    /// 处理器要标 [BypassesDataScope] 写明原因
    /// </remarks>
    public static IQueryable<T> WithoutDataScope<T>(this IQueryable<T> query)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.IgnoreQueryFilters([QueryFilters.DataScope]);
    }
}
