using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Application.Querying;

public static class QueryableExtensions
{
    /// <summary>跳过数据权限过滤（软删除过滤保持生效）；调用此方法的处理器须标注 [BypassesDataScope] 并说明原因。</summary>
    public static IQueryable<T> WithoutDataScope<T>(this IQueryable<T> query)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(query);
        return query.IgnoreQueryFilters([QueryFilters.DataScope]);
    }
}
