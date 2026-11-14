using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Organization;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Users.Grants;

/// <summary>
/// 校验角色授予的组织锚点，所有单元必须存在且已启用
/// </summary>
internal static class OrgAnchors
{
    public static async Task<Result> CheckAsync(IIdentityReads db, IReadOnlyList<OrgUnitId> anchors, CancellationToken ct)
    {
        List<OrgUnitId> distinct = [.. anchors.Distinct()];
        if (distinct.Count == 0)
        {
            return Result.Success();
        }

        var found = await db.OrgTrees.SelectMany(t => t.Units).Where(u => distinct.Contains(u.Id)).Select(u => new { u.Id, u.Status }).ToListAsync(ct);
        if (found.Count != distinct.Count)
        {
            return IdentityErrors.OrgUnitNotFound;
        }

        return found.Exists(u => u.Status != OrgUnitStatus.Active) ? IdentityErrors.OrgUnitDisabled : Result.Success();
    }
}
