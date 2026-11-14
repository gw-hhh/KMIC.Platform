namespace Kmic.Operations.Application.Messages.Instances;

/// <summary>
/// 实例心跳表里的一行，只用来读：由各实例定期登记
/// </summary>
public sealed record MessagingInstanceRow
{
    /// <summary>
    /// 实例 ID，每次进程启动重新生成
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 机器名
    /// </summary>
    public string Host { get; init; } = string.Empty;

    /// <summary>
    /// 角色，如 Api,Worker
    /// </summary>
    public string Roles { get; init; } = string.Empty;

    /// <summary>
    /// 构建号
    /// </summary>
    public string Build { get; init; } = string.Empty;

    /// <summary>
    /// 声明的订阅，JSON 数组
    /// </summary>
    public string Subscriptions { get; init; } = "[]";

    /// <summary>
    /// 启动时间
    /// </summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>
    /// 最近一次心跳
    /// </summary>
    public DateTimeOffset HeartbeatAt { get; init; }
}
