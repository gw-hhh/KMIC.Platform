using Kmic.BuildingBlocks.Application.Access;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Authorization;

/// <summary>
/// 查询操作权限判断所需的操作者信息
/// </summary>
internal static class OperatorFacts
{
    /// <summary>
    /// 操作者当前角色是否受保护
    /// </summary>
    public static async Task<bool> IsProtectedAsync(IIdentityReads db, IAccessContext access, CancellationToken ct) =>
        await db.Roles.Where(r => r.Id == access.RoleId).Select(r => r.IsProtected).SingleOrDefaultAsync(ct);
}
