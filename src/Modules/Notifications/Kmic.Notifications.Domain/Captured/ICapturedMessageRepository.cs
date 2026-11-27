using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Notifications.Domain.Captured;

/// <summary>捕获内容仓储</summary>
public interface ICapturedMessageRepository : IRepository<CapturedMessage, CapturedMessageId>
{
    /// <summary>删除早于 <paramref name="capturedBefore" /> 的捕获记录，返回删除条数</summary>
    Task<int> PurgeAsync(DateTimeOffset capturedBefore, CancellationToken ct);
}
