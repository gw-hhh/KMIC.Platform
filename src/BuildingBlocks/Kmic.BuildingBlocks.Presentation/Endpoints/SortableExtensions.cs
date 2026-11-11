using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 列表端点的排序声明
/// </summary>
public static class SortableExtensions
{
    /// <summary>
    /// 声明列表可以按哪些字段排序，OpenAPI 文档里据此列出，前端只能按这些字段排序
    /// </summary>
    /// <param name="builder">列表端点</param>
    /// <param name="sortMap">列表的排序字段表，与查询处理器传给 ToPagedAsync 的是同一个</param>
    /// <typeparam name="T">列表项类型</typeparam>
    public static RouteHandlerBuilder Sortable<T>(this RouteHandlerBuilder builder, SortMap<T> sortMap)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(sortMap);

        return builder.WithMetadata(new SortableMetadata(sortMap.Fields, sortMap.DefaultSort));
    }
}
