using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 查询处理器使用的框架查询扩展
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// 不按数据范围过滤，软删除过滤仍然生效；处理器必须标 [BypassesDataScope] 写明原因
    /// </summary>
    /// <remarks>
    /// 典型用途：编号唯一性要在全部数据中判断、候选人员计算、诊断
    /// </remarks>
    public static IQueryable<T> WithoutDataScope<T>(this IQueryable<T> query)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.IgnoreQueryFilters([QueryFilters.DataScope]);
    }
}
