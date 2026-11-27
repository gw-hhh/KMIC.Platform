using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Dictionaries;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Application.Dictionaries.Admin;

public sealed class ListDictionaryTypesQueryHandler(IConfigurationQueryDb db) : IQueryHandler<ListDictionaryTypesQuery, IReadOnlyList<DictionaryTypeListItem>>
{
    public async ValueTask<Result<IReadOnlyList<DictionaryTypeListItem>>> Handle(ListDictionaryTypesQuery query, CancellationToken cancellationToken)
    {
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
