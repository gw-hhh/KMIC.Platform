namespace Kmic.Operations.Application.Messages.Instances;

/// <summary>
/// 运行实例及其订阅信息
/// </summary>
/// <param name="Id">实例 ID</param>
/// <param name="Host">机器名</param>
/// <param name="Roles">运行角色，如 Api、Worker</param>
/// <param name="Build">构建号</param>
/// <param name="StartedAt">启动时间</param>
/// <param name="HeartbeatAt">最近一次心跳</param>
/// <param name="IsAlive">心跳是否仍在有效期内</param>
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
