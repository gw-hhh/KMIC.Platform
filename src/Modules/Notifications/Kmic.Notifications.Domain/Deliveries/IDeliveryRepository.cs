using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>
/// 投递的仓储
/// </summary>
public interface IDeliveryRepository : IRepository<Delivery, DeliveryId>
{
    /// <summary>
    /// 同一个模板、同一个去重键已经有的投递：收件人键与渠道的组合，处理时据此跳过重复的
    /// </summary>
    /// <param name="templateKey">模板键</param>
    /// <param name="dedupKey">业务给的去重键</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>形如“收件人键|渠道编码”的字符串集合</returns>
    Task<IReadOnlySet<string>> FindExistingAsync(string templateKey, string dedupKey, CancellationToken ct);

    /// <summary>
    /// 把失败的投递放回队列：次数清零，立即可以领取
    /// </summary>
    /// <remarks>
    /// 只有失败且没有放弃的投递可以重试。条件和更新在同一条语句里，两个人同时点只有一个生效
    /// </remarks>
    /// <param name="id">投递</param>
    /// <param name="createdNotBefore">投递必须不早于这个时间创建，含密通知用它挡住已经过了有效期的；不限制就不给</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>放回队列返回 true，条件不满足返回 false</returns>
    Task<bool> RetryAsync(DeliveryId id, DateTimeOffset? createdNotBefore, CancellationToken ct);

    /// <summary>
    /// 放弃一条失败的投递：状态仍是失败，加上放弃标记，不再出现在待处理清单里
    /// </summary>
    /// <param name="id">投递</param>
    /// <param name="reason">放弃的原因</param>
    /// <param name="now">现在，作为放弃的时间</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>标记成功返回 true，投递不是失败或已经放弃返回 false</returns>
    Task<bool> AbandonAsync(DeliveryId id, string reason, DateTimeOffset now, CancellationToken ct);

    /// <summary>
    /// 删除超过保留期的已完成投递：已发送、已跳过、已捕获，以及已放弃的失败投递；还在等人处理的失败投递保留
    /// </summary>
    /// <param name="createdBefore">创建时间早于它的才删除</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>删除的行数</returns>
    Task<int> PurgeAsync(DateTimeOffset createdBefore, CancellationToken ct);

    /// <summary>
    /// 清空含密模板的已完成投递里还留着的正文和短信参数，作为投递时清空的兜底
    /// </summary>
    /// <param name="templateKeys">含密模板的键</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>清空的行数</returns>
    Task<int> ClearSecretBodiesAsync(IReadOnlyCollection<string> templateKeys, CancellationToken ct);
}
