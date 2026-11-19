using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Organization;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 批量查询组织单元引用信息
/// </summary>
public sealed class GetOrgUnitReferencesQueryHandler(IIdentityQueryDb db) : IQueryHandler<GetOrgUnitReferencesQuery, IReadOnlyDictionary<Guid, ReferenceInfo>>
{
    /// <inheritdoc />
    public async ValueTask<Result<IReadOnlyDictionary<Guid, ReferenceInfo>>> Handle(GetOrgUnitReferencesQuery query, CancellationToken cancellationToken)
    {
        var rows = await db.OrgTrees
            .SelectMany(t => t.Units)
            .Where(u => query.Ids.Contains(u.Id))
            .Select(u => new { u.Id, u.Name, u.Code, u.Status })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(
            r => r.Id.Value,
            r => new ReferenceInfo(r.Id.Value, r.Name, r.Code, r.Status == OrgUnitStatus.Active ? ReferenceStatus.Active : ReferenceStatus.Disabled));
    }
}
