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
}
