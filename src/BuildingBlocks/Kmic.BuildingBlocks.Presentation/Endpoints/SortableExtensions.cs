using Kmic.BuildingBlocks.Application.Querying;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

public static class SortableExtensions
{
    /// <summary>声明列表端点允许的排序字段（导出到 OpenAPI，前端仅可按所列字段排序）</summary>
    public static RouteHandlerBuilder Sortable<T>(this RouteHandlerBuilder builder, SortMap<T> sortMap)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(sortMap);

        return builder.WithMetadata(new SortableMetadata(sortMap.Fields, sortMap.DefaultSort));
    }
}
