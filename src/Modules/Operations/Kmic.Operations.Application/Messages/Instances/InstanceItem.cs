namespace Kmic.Operations.Application.Messages.Instances;

/// <summary>
/// 一个实例：构建号、角色、心跳和它声明的订阅
/// </summary>
/// <param name="Id">实例 ID</param>
/// <param name="Host">机器名</param>
/// <param name="Roles">角色</param>
/// <param name="Build">构建号</param>
/// <param name="StartedAt">启动时间</param>
/// <param name="HeartbeatAt">最近一次心跳</param>
/// <param name="IsAlive">最近有心跳，当作还在运行</param>
/// <param name="Subscriptions">声明的订阅</param>
public sealed record InstanceItem(
    Guid Id,
    string Host,
    string Roles,
    string Build,
    DateTimeOffset StartedAt,
    DateTimeOffset HeartbeatAt,
    bool IsAlive,
    IReadOnlyList<InstanceSubscription> Subscriptions);
