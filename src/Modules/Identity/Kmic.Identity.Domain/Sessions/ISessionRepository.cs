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
    /// 须在事务中调用，先锁会话再读令牌状态，避免并发刷新读到旧状态
    /// </remarks>
    Task<Session?> FindByRefreshTokenForUpdateAsync(string tokenHash, CancellationToken ct);

    /// <summary>
    /// 查询涉及指定用户的未作废会话
    /// </summary>
    /// <remarks>
    /// 匹配会话用户或模拟会话的实际操作人，不检查绝对到期时间
    /// </remarks>
    Task<IReadOnlyList<Session>> ListActiveInvolvingAsync(UserId userId, CancellationToken ct);

    /// <summary>
    /// 删除绝对到期时间或作废时间早于 <paramref name="before" /> 的会话及其刷新令牌，返回删除的会话数
    /// </summary>
    Task<int> PurgeEndedAsync(DateTimeOffset before, CancellationToken ct);
}
