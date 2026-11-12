using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Identity.Domain.Errors;
using Kmic.Identity.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Identity.Infrastructure.Persistence.Repositories;

internal sealed class SessionRepository(IdentityDbContext context)
    : EfRepository<IdentityDbContext, Session, SessionId>(context), ISessionRepository
{
    protected override Error NotFound => IdentityErrors.SessionNotFound;

    // 刷新令牌总是一起加载
    protected override IQueryable<Session> Aggregate(IQueryable<Session> set) => set.Include(s => s.RefreshTokens);

    public async Task<Session?> FindByRefreshTokenForUpdateAsync(string tokenHash, CancellationToken ct)
    {
        var sessionId = await Context.Set<Session>()
            .Where(s => s.RefreshTokens.Any(t => t.TokenHash == tokenHash))
            .Select(s => (SessionId?)s.Id)
            .SingleOrDefaultAsync(ct);
        if (sessionId is null)
        {
            return null;
        }

        // 先锁会话行再读取：并发的两个刷新请求在这里排队，后到的读到的是前一个提交之后的令牌状态
        var locked = await GetForUpdateLockedAsync(sessionId.Value, null, ct);
        return locked.IsSuccess ? locked.Value : null;
    }

    public async Task<IReadOnlyList<Session>> ListActiveInvolvingAsync(UserId userId, CancellationToken ct) =>
        await Aggregate(Context.Set<Session>())
            .AsSplitQuery()
            .Where(s => s.Status == SessionStatus.Active && (s.UserId == userId || s.ImpersonatorUserId == userId))
            .ToListAsync(ct);
}
