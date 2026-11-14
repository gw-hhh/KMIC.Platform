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
    // 受保护角色持有人检查共用此咨询锁键，须与其他锁用途区分
    private const long ProtectedHolderLockKey = 0x4B4D_4943_0001;

    protected override Error NotFound => IdentityErrors.UserNotFound;

    protected override IQueryable<User> Aggregate(IQueryable<User> set) => set.Include(u => u.Grants).ThenInclude(g => g.Anchors);

    public async Task<User?> FindByAccountAsync(string account, CancellationToken ct) =>
        await Aggregate(Context.Set<User>()).AsSplitQuery().SingleOrDefaultAsync(u => u.Account == account, ct);

    public async Task<User?> FindByMobileAsync(string mobile, CancellationToken ct) =>
        await Aggregate(Context.Set<User>()).AsSplitQuery().SingleOrDefaultAsync(u => u.Mobile == mobile, ct);

    public async Task<bool> AnyNormalUserAsync(CancellationToken ct) =>
        await Context.Set<User>().AnyAsync(u => u.Kind == UserKind.Normal, ct);

    public async Task LockProtectedHolderChecksAsync(CancellationToken ct) =>
        await Context.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({ProtectedHolderLockKey})", ct);
}
