using Kmic.BuildingBlocks.Application.Access;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Application.Authorization;

internal static class OperatorFacts
{
    public static async Task<bool> IsProtectedAsync(IIdentityReads db, IAccessContext access, CancellationToken ct) =>
        await db.Roles.Where(r => r.Id == access.RoleId).Select(r => r.IsProtected).SingleOrDefaultAsync(ct);
}
