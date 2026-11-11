namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// 框架注册的命名查询过滤器
/// </summary>
public static class QueryFilters
{
    /// <summary>
    /// 软删除；回收站用 IgnoreQueryFilters([QueryFilters.SoftDelete]) 只关闭它，数据范围仍然生效
    /// </summary>
    public const string SoftDelete = nameof(SoftDelete);

    /// <summary>
    /// 数据范围；只能经 WithoutDataScope() 关闭，处理器同时标 [BypassesDataScope]
    /// </summary>
    public const string DataScope = nameof(DataScope);
}
