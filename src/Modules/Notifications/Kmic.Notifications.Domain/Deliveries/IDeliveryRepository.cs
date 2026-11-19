using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>
/// 投递仓储
/// </summary>
public interface IDeliveryRepository : IRepository<Delivery, DeliveryId>
{
    /// <summary>
    /// 查询同一模板和去重键下已有投递的收件人键与渠道组合
    /// </summary>
    /// <returns>格式为“收件人键|渠道编码”的字符串集合</returns>
    Task<IReadOnlySet<string>> FindExistingAsync(string templateKey, string dedupKey, CancellationToken ct);

    /// <summary>
    /// 将失败的投递重新加入队列，领取次数清零
    /// </summary>
    /// <returns>成功加入队列返回 true；条件不满足返回 false</returns>
    Task<bool> RetryAsync(DeliveryId id, CancellationToken ct);

    /// <summary>
    /// 放弃失败的投递，保留失败状态并记录原因和时间
    /// </summary>
    /// <returns>标记成功返回 true；投递不是失败状态或已放弃时返回 false</returns>
    Task<bool> AbandonAsync(DeliveryId id, string reason, DateTimeOffset now, CancellationToken ct);

    /// <summary>
    /// 删除超过保留期的已完成投递
    /// </summary>
    /// <returns>删除条数</returns>
    Task<int> PurgeAsync(DateTimeOffset createdBefore, CancellationToken ct);

    /// <summary>
    /// 清空指定含密模板的残留正文和短信参数
    /// </summary>
    /// <returns>清空的投递数</returns>
    Task<int> ClearSecretBodiesAsync(IReadOnlyCollection<string> templateKeys, CancellationToken ct);
}
