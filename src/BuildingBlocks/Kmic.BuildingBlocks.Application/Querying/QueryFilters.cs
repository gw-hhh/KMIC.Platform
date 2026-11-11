namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 查询过滤器名称
/// </summary>
public static class QueryFilters
{
    /// <summary>
    /// 软删除
    /// </summary>
    /// <remarks>
    /// 回收站用 IgnoreQueryFilters([QueryFilters.SoftDelete]) 只关掉它，数据权限仍然生效
    /// </remarks>
    public const string SoftDelete = nameof(SoftDelete);

    /// <summary>
    /// 数据权限
    /// </summary>
    /// <remarks>
    /// 只能用 WithoutDataScope() 关掉
    /// </remarks>
    public const string DataScope = nameof(DataScope);
}
