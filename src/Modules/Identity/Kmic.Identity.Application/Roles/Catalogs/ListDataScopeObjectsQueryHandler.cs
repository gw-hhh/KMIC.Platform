using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Identity.Application.Roles.Catalogs;

/// <summary>
/// 查询数据范围对象目录
/// </summary>
public sealed class ListDataScopeObjectsQueryHandler(DataScopeObjectCatalog catalog) : IQueryHandler<ListDataScopeObjectsQuery, IReadOnlyList<DataScopeObjectItem>>
{
    /// <inheritdoc />
    public ValueTask<Result<IReadOnlyList<DataScopeObjectItem>>> Handle(ListDataScopeObjectsQuery query, CancellationToken cancellationToken)
    {
        List<DataScopeObjectItem> items =
        [
            .. catalog.All
                .OrderBy(o => o.Module, StringComparer.Ordinal)
                .ThenBy(o => o.Code, StringComparer.Ordinal)
                .Select(o => new DataScopeObjectItem(o.Code, o.Module, o.DisplayName)),
        ];
        Result<IReadOnlyList<DataScopeObjectItem>> result = items;
        return ValueTask.FromResult(result);
    }
}
