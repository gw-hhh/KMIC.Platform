using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Organization;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Users.Grants;

/// <summary>
/// 授予上的锚点检查：每个锚点都必须是存在的、启用的组织单元
/// </summary>
internal static class OrgAnchors
{
    public static async Task<Result> CheckAsync(IIdentityQueryDb db, IReadOnlyList<OrgUnitId> anchors, CancellationToken ct)
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
