using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Organization;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Infrastructure.Persistence.Repositories;

internal sealed class OrgTreeRepository(IdentityDbContext context)
    : EfRepository<IdentityDbContext, OrgTree, OrgTreeId>(context), IOrgTreeRepository
{
    protected override Error NotFound => IdentityErrors.OrgUnitNotFound;

    // 整棵树总是一起加载
    protected override IQueryable<OrgTree> Aggregate(IQueryable<OrgTree> set) => set.Include(t => t.Units);
}
