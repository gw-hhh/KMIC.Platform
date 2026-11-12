using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Dictionaries;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Application.Dictionaries.Admin;

/// <summary>
/// 处理 <see cref="ListDictionaryTypesQuery"/>
/// </summary>
public sealed class ListDictionaryTypesQueryHandler(IConfigurationQueryDb db) : IQueryHandler<ListDictionaryTypesQuery, IReadOnlyList<DictionaryTypeListItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<IReadOnlyList<DictionaryTypeListItem>>> Handle(ListDictionaryTypesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        List<DictionaryTypeListItem> items = await db.DictionaryTypes
            .OrderBy(t => t.OwnerModule)
            .ThenBy(t => t.Code)
            .Select(t => new DictionaryTypeListItem(
                t.Code,
                t.DisplayName,
                t.OwnerModule,
                t.IsHierarchical,
                t.Status,
                t.Items.Count,
                t.Items.Count(i => i.Status == DictionaryItemStatus.Active),
                t.ConcurrencyVersion))
            .ToListAsync(cancellationToken);
        Result<IReadOnlyList<DictionaryTypeListItem>> result = items;
        return result;
    }
}
