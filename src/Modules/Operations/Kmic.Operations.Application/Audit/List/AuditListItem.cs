namespace Kmic.Operations.Application.Audit.List;

/// <summary>
/// 审计记录列表项
/// </summary>
/// <param name="Id">审计记录 ID</param>
/// <param name="OccurredAt">操作发生时间</param>
/// <param name="Module">产生审计的模块</param>
/// <param name="Action">操作名</param>
/// <param name="Origin">请求来源</param>
/// <param name="ActorUserId">操作人 ID</param>
/// <param name="ActorName">操作人的名称，用户已经不存在时为空</param>
/// <param name="ActualUserId">模拟身份时的实际操作人 ID</param>
/// <param name="ActualName">实际操作人的名称</param>
/// <param name="TraceId">链路追踪 ID</param>
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
