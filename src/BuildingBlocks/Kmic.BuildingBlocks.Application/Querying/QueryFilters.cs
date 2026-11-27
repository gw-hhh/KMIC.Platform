namespace Kmic.BuildingBlocks.Application.Querying;

public static class QueryFilters
{
    /// <summary>软删除过滤器；回收站查询可通过 IgnoreQueryFilters([QueryFilters.SoftDelete]) 单独禁用。</summary>
    public const string SoftDelete = nameof(SoftDelete);

    /// <summary>数据权限过滤器；仅允许通过 WithoutDataScope() 禁用。</summary>
    public const string DataScope = nameof(DataScope);
}
