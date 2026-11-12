using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Roles;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 处理 <see cref="GetRoleReferencesQuery"/>
/// </summary>
/// <remarks>
/// 角色不删除，只停用：审计、历史里显示的角色名，停用之后仍然解析得出来，状态是“已停用”。角色没有编码
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
