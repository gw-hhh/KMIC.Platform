namespace Kmic.Operations.Application.Messages.Instances;

/// <summary>实例心跳表只读查询结果</summary>
public sealed record MessagingInstanceRow
{
    /// <summary>实例 ID，每次进程启动重新生成</summary>
    public Guid Id { get; init; }

    public string Host { get; init; } = string.Empty;

    /// <summary>节点角色，如 Api,Worker</summary>
    public string Roles { get; init; } = string.Empty;

    public string Build { get; init; } = string.Empty;

    /// <summary>声明的订阅（JSON 数组）</summary>
    public string Subscriptions { get; init; } = "[]";

    public DateTimeOffset StartedAt { get; init; }

    public DateTimeOffset HeartbeatAt { get; init; }
}
