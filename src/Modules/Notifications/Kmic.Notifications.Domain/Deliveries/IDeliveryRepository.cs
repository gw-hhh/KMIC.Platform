using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>投递仓储</summary>
public interface IDeliveryRepository : IRepository<Delivery, DeliveryId>
{
    /// <summary>查询同一模板和去重键下已存在的“收件人键|渠道编码”组合</summary>
    Task<IReadOnlySet<string>> FindExistingAsync(string templateKey, string dedupKey, CancellationToken ct);

    /// <summary>将失败投递重新入队并清零领取次数；不满足条件返回 false</summary>
    Task<bool> RetryAsync(DeliveryId id, CancellationToken ct);

    /// <summary>放弃失败投递并记录原因与时间；非失败状态或已放弃时返回 false</summary>
    Task<bool> AbandonAsync(DeliveryId id, string reason, DateTimeOffset now, CancellationToken ct);

    /// <summary>删除超过保留期的已完成投递，返回删除条数</summary>
    Task<int> PurgeAsync(DateTimeOffset createdBefore, CancellationToken ct);

    /// <summary>清空指定敏感模板的残留正文与短信参数，返回清空条数</summary>
    Task<int> ClearSecretBodiesAsync(IReadOnlyCollection<string> templateKeys, CancellationToken ct);
}
