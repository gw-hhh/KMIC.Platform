using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Contracts.Auditing;

/// <summary>
/// 审计记录
/// </summary>
/// <remarks>
/// 框架不能引用 Operations 模块，所以定义在这里
/// </remarks>
[IntegrationMessage("Framework.AuditRecorded")]
public sealed record AuditRecorded : IIntegrationEvent
{
    /// <summary>
    /// 审计记录 ID
    /// </summary>
    public required Guid AuditId { get; init; }

    /// <summary>
    /// 操作时间
    /// </summary>
    public required DateTimeOffset OccurredAt { get; init; }

    /// <summary>
    /// 模块
    /// </summary>
    public required string Module { get; init; }

    /// <summary>
    /// 操作名
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// 请求来源
    /// </summary>
    public required string Origin { get; init; }

    /// <summary>
    /// 操作人
    /// </summary>
    public Guid? ActorUserId { get; init; }

    /// <summary>
    /// 实际操作人
    /// </summary>
    public Guid? ActualUserId { get; init; }

    /// <summary>
    /// 角色
    /// </summary>
    public Guid? RoleId { get; init; }

    /// <summary>
    /// 会话
    /// </summary>
    public Guid? SessionId { get; init; }

    /// <summary>
    /// 发起原始请求的用户
    /// </summary>
    public Guid? OnBehalfOfUserId { get; init; }

    /// <summary>
    /// 客户端 IP
    /// </summary>
    public string? ClientIp { get; init; }

    /// <summary>
    /// 追踪 ID，与错误响应和日志中的 traceId 一致
    /// </summary>
    public required string TraceId { get; init; }

    /// <summary>
    /// 操作对象
    /// </summary>
    public required IReadOnlyList<AuditTarget> Targets { get; init; }

    /// <summary>
    /// 字段变更
    /// </summary>
    public required IReadOnlyList<EntityChange> Changes { get; init; }

    /// <summary>
    /// 补充说明，如导出的筛选条件
    /// </summary>
    public required IReadOnlyDictionary<string, string> Details { get; init; }
}
