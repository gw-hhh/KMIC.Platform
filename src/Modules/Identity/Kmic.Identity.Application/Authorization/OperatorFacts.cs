using Kmic.BuildingBlocks.Application.Access;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Authorization;

/// <summary>
/// 管理命令与查询交给资源策略的、关于操作者的事实
/// </summary>
internal static class OperatorFacts
{
    /// <summary>
    /// 操作者当前的角色是不是受保护角色
    /// </summary>
    public static async Task<bool> IsProtectedAsync(IIdentityQueryDb db, IAccessContext access, CancellationToken ct) =>
        await db.Roles.Where(r => r.Id == access.RoleId).Select(r => r.IsProtected).SingleOrDefaultAsync(ct);
}
