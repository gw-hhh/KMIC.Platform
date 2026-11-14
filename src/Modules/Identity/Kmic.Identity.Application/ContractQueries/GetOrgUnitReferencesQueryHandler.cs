using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Organization;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 批量查询组织单元引用信息
/// </summary>
/// <remarks>
/// 停用的组织单元仍返回名称、单元编码及停用状态，供历史记录显示
/// </remarks>
public sealed class GetOrgUnitReferencesQueryHandler(IIdentityQueryDb db) : IQueryHandler<GetOrgUnitReferencesQuery, IReadOnlyDictionary<Guid, ReferenceInfo>>
{
    /// <inheritdoc />
    public async ValueTask<Result<IReadOnlyDictionary<Guid, ReferenceInfo>>> Handle(GetOrgUnitReferencesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

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
