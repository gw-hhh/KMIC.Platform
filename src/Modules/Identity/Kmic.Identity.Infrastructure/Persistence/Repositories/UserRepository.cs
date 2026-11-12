using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(IdentityDbContext context)
    : EfRepository<IdentityDbContext, User, UserId>(context), IUserRepository
{
    protected override Error NotFound => IdentityErrors.UserNotFound;

    // 授予与锚点总是一起加载
    protected override IQueryable<User> Aggregate(IQueryable<User> set) => set.Include(u => u.Grants).ThenInclude(g => g.Anchors);

    public async Task<User?> FindByAccountAsync(string account, CancellationToken ct) =>
        await Aggregate(Context.Set<User>()).AsSplitQuery().SingleOrDefaultAsync(u => u.Account == account, ct);

    public async Task<User?> FindByMobileAsync(string mobile, CancellationToken ct) =>
        await Aggregate(Context.Set<User>()).AsSplitQuery().SingleOrDefaultAsync(u => u.Mobile == mobile, ct);

    public async Task<bool> AnyNormalUserAsync(CancellationToken ct) =>
        await Context.Set<User>().AnyAsync(u => u.Kind == UserKind.Normal, ct);
}
