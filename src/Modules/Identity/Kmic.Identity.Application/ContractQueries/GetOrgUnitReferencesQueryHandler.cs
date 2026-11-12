using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Organization;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 处理 <see cref="GetOrgUnitReferencesQuery"/>
/// </summary>
/// <remarks>
/// 组织单元不删除，只停用：业务数据里保存着的所属组织，停用之后仍然解析得出来，状态是“已停用”。编码取单元编码
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
