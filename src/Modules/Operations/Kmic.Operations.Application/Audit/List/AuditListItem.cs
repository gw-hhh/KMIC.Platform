namespace Kmic.Operations.Application.Audit.List;

/// <summary>
/// 审计列表里的一条
/// </summary>
/// <param name="Id">审计记录 ID</param>
/// <param name="OccurredAt">操作发生的时间</param>
/// <param name="Module">产生审计的模块</param>
/// <param name="Action">操作名</param>
/// <param name="Origin">请求来源，如 Http、ScheduledJob</param>
/// <param name="ActorUserId">操作人</param>
/// <param name="ActorName">操作人的名称；用户已经不存在时为空</param>
/// <param name="ActualUserId">模拟登录时真正动手的人</param>
/// <param name="ActualName">实际操作人的名称</param>
/// <param name="TraceId">链路追踪 ID，可以到日志与链路里查这次操作</param>
/// <param name="Targets">操作对象</param>
public sealed record AuditListItem(
    Guid Id,
    DateTimeOffset OccurredAt,
    string Module,
    string Action,
    string Origin,
    Guid? ActorUserId,
    string? ActorName,
    Guid? ActualUserId,
    string? ActualName,
    string TraceId,
    IReadOnlyList<AuditTargetItem> Targets);
