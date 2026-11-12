using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Roles;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Infrastructure.Persistence.Repositories;

internal sealed class RoleRepository(IdentityDbContext context)
    : EfRepository<IdentityDbContext, Role, RoleId>(context), IRoleRepository
{
    protected override Error NotFound => IdentityErrors.RoleNotFound;

    // 权限点与数据范围覆盖总是一起加载
    protected override IQueryable<Role> Aggregate(IQueryable<Role> set) => set.Include(r => r.Permissions).Include(r => r.DataScopes);

    public async Task<IReadOnlyList<Role>> ListWithPermissionsAsync(IReadOnlyCollection<string> permissionCodes, CancellationToken ct) =>
        await Aggregate(Context.Set<Role>()).AsSplitQuery()
            .Where(r => r.Permissions.Any(p => permissionCodes.Contains(p.PermissionCode)))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Role>> ListWithDataScopeOverridesAsync(IReadOnlyCollection<string> scopeObjects, CancellationToken ct) =>
        await Aggregate(Context.Set<Role>()).AsSplitQuery()
            .Where(r => r.DataScopes.Any(s => scopeObjects.Contains(s.ScopeObject)))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Role>> ListGrantingAllAsync(CancellationToken ct) =>
        await Aggregate(Context.Set<Role>()).AsSplitQuery().Where(r => r.GrantsAllPermissions).ToListAsync(ct);
}
