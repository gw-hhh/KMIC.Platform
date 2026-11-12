using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Identity.Application.Roles.Catalogs;

/// <summary>
/// 处理 <see cref="ListPermissionsQuery"/>
/// </summary>
public sealed class ListPermissionsQueryHandler(PermissionCatalog catalog) : IQueryHandler<ListPermissionsQuery, IReadOnlyList<PermissionItem>>
{
    /// <inheritdoc />
    public ValueTask<Result<IReadOnlyList<PermissionItem>>> Handle(ListPermissionsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        List<PermissionItem> items =
        [
            .. catalog.All
                .OrderBy(p => p.Module, StringComparer.Ordinal)
                .ThenBy(p => p.Group, StringComparer.Ordinal)
                .ThenBy(p => p.Code, StringComparer.Ordinal)
                .Select(p => new PermissionItem(p.Code, p.Module, p.Group, p.DisplayName, p.Sensitive)),
        ];
        Result<IReadOnlyList<PermissionItem>> result = items;
        return ValueTask.FromResult(result);
    }
}
