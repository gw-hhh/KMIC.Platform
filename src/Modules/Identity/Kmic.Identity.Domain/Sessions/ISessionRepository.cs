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
}
