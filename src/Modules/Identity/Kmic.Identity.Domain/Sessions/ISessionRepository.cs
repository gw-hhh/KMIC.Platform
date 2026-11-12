using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Identity.Domain.Sessions;

/// <summary>
/// 会话的仓储
/// </summary>
public interface ISessionRepository : IRepository<Session, SessionId>
{
    /// <summary>
    /// 按刷新令牌的哈希找到会话并锁住这一行，刷新时用：两个并发刷新请求被串行化，第二个落入宽限期判断
    /// </summary>
    /// <remarks>
    /// 只在事务里调用。找不到返回空
    /// </remarks>
    Task<Session?> FindByRefreshTokenForUpdateAsync(string tokenHash, CancellationToken ct);

    /// <summary>
    /// 某个用户仍然有效的全部会话，含他作为实际操作人发起的模拟会话
    /// </summary>
    Task<IReadOnlyList<Session>> ListActiveInvolvingAsync(UserId userId, CancellationToken ct);

    /// <summary>
    /// 删除已经结束的会话（连同它的刷新令牌）：超过绝对上限，或作废的时间早于 <paramref name="before" />，返回删除的会话数
    /// </summary>
    /// <remarks>
    /// 定时清理用。会话表只保留“最近发生了什么”，更久远的登录记录在安全事件里
    /// </remarks>
    Task<int> PurgeEndedAsync(DateTimeOffset before, CancellationToken ct);
}
