using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Notifications.Domain.Captured;

/// <summary>
/// 捕获内容的仓储
/// </summary>
public interface ICapturedMessageRepository : IRepository<CapturedMessage, CapturedMessageId>
{
    /// <summary>
    /// 删除超过保留期的捕获内容
    /// </summary>
    /// <param name="capturedBefore">捕获时间早于它的才删除</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>删除的行数</returns>
    Task<int> PurgeAsync(DateTimeOffset capturedBefore, CancellationToken ct);
}
