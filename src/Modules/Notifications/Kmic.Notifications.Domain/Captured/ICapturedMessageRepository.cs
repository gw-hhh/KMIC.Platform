using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Notifications.Domain.Captured;

/// <summary>
/// 捕获内容的仓储
/// </summary>
public interface ICapturedMessageRepository : IRepository<CapturedMessage, CapturedMessageId>
{
    /// <summary>
    /// 删除捕获时间早于 <paramref name="capturedBefore" /> 的内容
    /// </summary>
    /// <returns>删除条数</returns>
    Task<int> PurgeAsync(DateTimeOffset capturedBefore, CancellationToken ct);
}
