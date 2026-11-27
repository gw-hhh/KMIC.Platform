namespace Kmic.Identity.Application.SecurityEvents;

/// <summary>框架安全事件的只读查询结果。</summary>
public sealed record SecurityEventRow
{
    /// <summary>事件 ID</summary>
    public Guid Id { get; init; }

    /// <summary>发生时间</summary>
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>类型，如 LoginFailed、AuthorizationDenied</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>严重程度：0 提示、1 警告、2 严重</summary>
    public short Severity { get; init; }

    /// <summary>原因编码</summary>
    public string? Reason { get; init; }

    /// <summary>用户 ID</summary>
    public Guid? UserId { get; init; }

    /// <summary>实际操作人 ID</summary>
    public Guid? ActualUserId { get; init; }

    /// <summary>操作时使用的角色 ID</summary>
    public Guid? RoleId { get; init; }

    /// <summary>会话 ID</summary>
    public Guid? SessionId { get; init; }

    /// <summary>事件关联的账号</summary>
    public string? AttemptedAccount { get; init; }

    /// <summary>客户端 IP</summary>
    public string? ClientIp { get; init; }

    /// <summary>资源类型</summary>
    public string? ResourceType { get; init; }

    /// <summary>资源 ID</summary>
    public Guid? ResourceId { get; init; }

    /// <summary>补充说明，JSON 文本</summary>
    public string? Details { get; init; }

    /// <summary>链路追踪 ID</summary>
    public string TraceId { get; init; } = string.Empty;
}
