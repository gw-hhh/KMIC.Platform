using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Sample.Application.Specimens.List;

/// <summary>
/// 处理 <see cref="ListSpecimensQuery"/>
/// </summary>
public sealed class ListSpecimensQueryHandler(ISampleQueryDb db) : IQueryHandler<ListSpecimensQuery, PagedResult<SpecimenListItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<PagedResult<SpecimenListItem>>> Handle(ListSpecimensQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var specimens = db.Specimens.AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            specimens = specimens.Where(s => KmicFunctions.ContainsText(s.Number, query.Q) || KmicFunctions.ContainsText(s.Title, query.Q));
        }

        if (query.Status is { Count: > 0 })
        {
            specimens = specimens.Where(s => query.Status.Contains(s.Status.Code));
        }

        return await specimens
            .Select(s => new SpecimenListItem(s.Id, s.Number, s.Title, s.Status, EF.Property<DateTimeOffset>(s, AuditColumns.CreatedAt)))
            .ToPagedAsync(query.Page, query.Sort, SpecimenSort.Map, cancellationToken);
    }
}
