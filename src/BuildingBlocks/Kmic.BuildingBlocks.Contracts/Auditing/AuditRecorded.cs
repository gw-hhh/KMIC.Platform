using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Contracts.Auditing;

/// <summary>
/// 一次被审计的操作：随业务事务写入本模块 Outbox，唯一订阅方是 Operations 的 AuditLog
/// </summary>
/// <remarks>
/// 框架消息，是“消息名以本模块名开头”的唯一例外；定义在这里是因为框架不能引用 Operations 模块
/// </remarks>
[IntegrationMessage("Framework.AuditRecorded")]
public sealed record AuditRecorded : IIntegrationEvent
{
    /// <summary>
    /// 审计记录 ID
    /// </summary>
    public required Guid AuditId { get; init; }

    /// <summary>
    /// 操作发生的时刻（UTC），同一命令内取同一时刻
    /// </summary>
    public required DateTimeOffset OccurredAt { get; init; }

    /// <summary>
    /// 执行操作的模块名
    /// </summary>
    public required string Module { get; init; }

    /// <summary>
    /// 动作名，默认为“模块.命令名去掉 Command 后缀”，可用 [Audited(Action = ...)] 固定
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// 调用来源：Http、Inbox、ScheduledJob、BackgroundJob、Startup、ModuleCall
    /// </summary>
    public required string Origin { get; init; }

    /// <summary>
    /// 操作人；模拟身份时为被模拟的用户，系统身份时为空
    /// </summary>
    public Guid? ActorUserId { get; init; }

    /// <summary>
    /// 模拟身份时的实际操作人，否则为空
    /// </summary>
    public Guid? ActualUserId { get; init; }

    /// <summary>
    /// 操作时的当前角色
    /// </summary>
    public Guid? RoleId { get; init; }

    /// <summary>
    /// 操作所在的会话
    /// </summary>
    public Guid? SessionId { get; init; }

    /// <summary>
    /// 事件处理中再次操作时，原始请求的用户
    /// </summary>
    public Guid? OnBehalfOfUserId { get; init; }

    /// <summary>
    /// HTTP 来源时经转发头处理后的客户端地址，其他来源为空
    /// </summary>
    public string? ClientIp { get; init; }

    /// <summary>
    /// 追踪 ID，与错误响应和日志中的 traceId 一致
    /// </summary>
    public required string TraceId { get; init; }

    /// <summary>
    /// 被操作的资源：本事务中修改过的聚合根，以及命令补充的目标
    /// </summary>
    public required IReadOnlyList<AuditTarget> Targets { get; init; }

    /// <summary>
    /// 字段级变更，敏感字段已按映射配置遮盖或隐藏
    /// </summary>
    public required IReadOnlyList<EntityChange> Changes { get; init; }

    /// <summary>
    /// 命令补充的说明，如导出的筛选条件、批量操作的条数；不含命令参数
    /// </summary>
    public required IReadOnlyDictionary<string, string> Details { get; init; }
}
