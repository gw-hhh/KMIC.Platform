using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.BuildingBlocks.Contracts.Auditing;

/// <summary>审计记录。</summary>
[IntegrationMessage("Framework.AuditRecorded")]
public sealed record AuditRecorded : IIntegrationEvent
{
    /// <summary>审计记录 ID。</summary>
    public required Guid AuditId { get; init; }

    /// <summary>操作时间。</summary>
    public required DateTimeOffset OccurredAt { get; init; }

    /// <summary>所属模块。</summary>
    public required string Module { get; init; }

    /// <summary>操作名称。</summary>
    public required string Action { get; init; }

    /// <summary>请求来源。</summary>
    public required string Origin { get; init; }

    /// <summary>操作用户 ID。</summary>
    public Guid? ActorUserId { get; init; }

    /// <summary>实际操作用户 ID。</summary>
    public Guid? ActualUserId { get; init; }

    /// <summary>角色 ID。</summary>
    public Guid? RoleId { get; init; }

    /// <summary>会话 ID。</summary>
    public Guid? SessionId { get; init; }

    /// <summary>原始请求用户 ID。</summary>
    public Guid? OnBehalfOfUserId { get; init; }

    /// <summary>客户端 IP。</summary>
    public string? ClientIp { get; init; }

    /// <summary>追踪 ID，与错误响应和日志中的 traceId 一致。</summary>
    public required string TraceId { get; init; }

    /// <summary>操作对象列表。</summary>
    public required IReadOnlyList<AuditTarget> Targets { get; init; }

    /// <summary>实体变更列表。</summary>
    public required IReadOnlyList<EntityChange> Changes { get; init; }

    /// <summary>补充说明，例如导出的筛选条件。</summary>
    public required IReadOnlyDictionary<string, string> Details { get; init; }
}
