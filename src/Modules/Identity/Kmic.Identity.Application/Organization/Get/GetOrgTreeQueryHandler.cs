using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Organization.Get;

/// <summary>
/// 处理 <see cref="GetOrgTreeQuery"/>
/// </summary>
public sealed class GetOrgTreeQueryHandler(IIdentityQueryDb db) : IQueryHandler<GetOrgTreeQuery, OrgTreeResponse>
{
    /// <inheritdoc />
    public async ValueTask<Result<OrgTreeResponse>> Handle(GetOrgTreeQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var tree = await db.OrgTrees
            .Select(t => new
            {
                t.ConcurrencyVersion,
                Units = t.Units.Select(u => new OrgUnitItem(u.Id, u.ParentId, u.Name, u.Code, u.Status, u.SortOrder)).ToList(),
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (tree is null)
        {
            return IdentityErrors.OrgUnitNotFound;
        }

        return new OrgTreeResponse(
            tree.ConcurrencyVersion,
            [.. tree.Units.OrderBy(u => u.ParentId?.Value ?? Guid.Empty).ThenBy(u => u.SortOrder).ThenBy(u => u.Name, StringComparer.Ordinal)]);
    }
}
