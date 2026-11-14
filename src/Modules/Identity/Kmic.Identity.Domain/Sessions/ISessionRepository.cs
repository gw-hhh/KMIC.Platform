using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Identity.Domain.Sessions;

/// <summary>
/// 会话的仓储
/// </summary>
public interface ISessionRepository : IRepository<Session, SessionId>
{
    /// <summary>
    /// 按刷新令牌哈希查找并锁定会话
    /// </summary>
    /// <remarks>
    /// 须在事务中调用，锁持有至事务结束；取得会话锁后再读取令牌状态，避免并发刷新读到旧状态。找不到时返回 null
    /// </remarks>
    Task<Session?> FindByRefreshTokenForUpdateAsync(string tokenHash, CancellationToken ct);

    /// <summary>
    /// 查询状态为有效的用户会话，包括该用户发起的模拟会话；不检查绝对到期时间
    /// </summary>
    Task<IReadOnlyList<Session>> ListActiveInvolvingAsync(UserId userId, CancellationToken ct);

    /// <summary>
    /// 删除绝对到期时间或作废时间早于 <paramref name="before" /> 的会话及其刷新令牌，返回删除的会话数
    /// </summary>
    Task<int> PurgeEndedAsync(DateTimeOffset before, CancellationToken ct);
}
