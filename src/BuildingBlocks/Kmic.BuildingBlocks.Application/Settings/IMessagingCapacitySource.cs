namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>数据库已分配容量的配置来源，供消息容量保护读取；未注册或未配置时跳过容量检查。</summary>
public interface IMessagingCapacitySource
{
    /// <summary>读取容量配置及最后更新时间，未配置时返回 null。</summary>
    Task<MessagingCapacity?> ReadAsync(CancellationToken ct);
}
