using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Contracts;
using Kmic.Configuration.Domain.Dictionaries;
using Kmic.Configuration.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Application.Dictionaries.Admin;

/// <summary>
/// 处理 <see cref="GetDictionaryTypeQuery"/>
/// </summary>
public sealed class GetDictionaryTypeQueryHandler(IConfigurationQueryDb db) : IQueryHandler<GetDictionaryTypeQuery, DictionaryTypeDetail>
{
    /// <inheritdoc />
    public async ValueTask<Result<DictionaryTypeDetail>> Handle(GetDictionaryTypeQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var type = await db.DictionaryTypes
            .Where(t => t.Code == query.TypeCode)
            .Select(t => new
            {
                t.Code,
                t.DisplayName,
                t.OwnerModule,
                t.IsHierarchical,
                t.Status,
                t.ConcurrencyVersion,
                Items = t.Items.Select(i => new { i.Code, i.Name, i.ParentCode, i.SortOrder, i.Status }).ToList(),
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (type is null)
        {
            return ConfigurationErrors.DictionaryTypeNotFound;
        }

        return new DictionaryTypeDetail(
            type.Code,
            type.DisplayName,
            type.OwnerModule,
            type.IsHierarchical,
            type.Status,
            type.ConcurrencyVersion,
            [
                .. type.Items
                    .Select(i => new DictionaryEntry(i.Code, i.Name, i.ParentCode, i.SortOrder, i.Status == DictionaryItemStatus.Active))
                    .OrderBy(i => i.ParentCode, StringComparer.Ordinal)
                    .ThenBy(i => i.SortOrder),
            ]);
    }
}
