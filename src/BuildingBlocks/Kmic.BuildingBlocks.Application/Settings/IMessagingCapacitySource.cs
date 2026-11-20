namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 数据库的已分配容量的来源
/// </summary>
/// <remarks>
/// 容量存成系统设置，由声明这项设置的模块（Operations）实现。每次都要读最新值，也要知道它多久没更新。没有登记实现，或设置从未写过时，容量保护不判断
/// </remarks>
public interface IMessagingCapacitySource
{
    /// <summary>
    /// 读取容量及其最后更新时间，没有设置容量时返回空
    /// </summary>
    Task<MessagingCapacity?> ReadAsync(CancellationToken ct);
}
