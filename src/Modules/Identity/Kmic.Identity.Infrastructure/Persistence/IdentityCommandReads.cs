using Kmic.Identity.Application;
using Kmic.Identity.Domain.Organization;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Infrastructure.Persistence;

// 复用写上下文连接与事务执行不跟踪查询，供命令校验使用
internal sealed class IdentityCommandReads(IdentityDbContext context) : IIdentityCommandReads
{
    public IQueryable<User> Users => context.Set<User>().AsNoTracking();

    public IQueryable<Role> Roles => context.Set<Role>().AsNoTracking();

    public IQueryable<OrgTree> OrgTrees => context.Set<OrgTree>().AsNoTracking();
}
