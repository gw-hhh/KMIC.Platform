using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Roles;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 批量查询角色引用信息
/// </summary>
/// <remarks>
/// 停用角色仍返回名称及停用状态，供历史记录显示；角色编码为 null
/// </remarks>
public sealed class GetRoleReferencesQueryHandler(IIdentityQueryDb db) : IQueryHandler<GetRoleReferencesQuery, IReadOnlyDictionary<Guid, ReferenceInfo>>
{
    /// <inheritdoc />
    public async ValueTask<Result<IReadOnlyDictionary<Guid, ReferenceInfo>>> Handle(GetRoleReferencesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var rows = await db.Roles
            .Where(r => query.Ids.Contains(r.Id))
            .Select(r => new { r.Id, r.Name, r.Status })
            .ToListAsync(cancellationToken);
        return rows.ToDictionary(
            r => r.Id.Value,
            r => new ReferenceInfo(r.Id.Value, r.Name, null, r.Status == RoleStatus.Active ? ReferenceStatus.Active : ReferenceStatus.Disabled));
    }
}
