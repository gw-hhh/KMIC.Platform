using Kmic.Identity.Application;
using Kmic.Identity.Domain.Organization;
using Kmic.Identity.Domain.Roles;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Infrastructure.Persistence;

/// <summary>
/// 从写上下文读取命令判断所需数据，与仓储共用连接和事务，不跟踪实体
/// </summary>
internal sealed class IdentityCommandReads(IdentityDbContext context) : IIdentityCommandReads
{
    /// <inheritdoc />
    public IQueryable<User> Users => context.Set<User>().AsNoTracking();

    /// <inheritdoc />
    public IQueryable<Role> Roles => context.Set<Role>().AsNoTracking();

    /// <inheritdoc />
    public IQueryable<OrgTree> OrgTrees => context.Set<OrgTree>().AsNoTracking();
}
