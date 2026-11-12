using System.Text.Json;

namespace Kmic.Operations.Application.Audit.Get;

/// <summary>
/// 审计记录的详情
/// </summary>
/// <param name="Id">审计记录 ID</param>
/// <param name="OccurredAt">操作发生的时间</param>
/// <param name="ReceivedAt">汇总到这里的时间</param>
/// <param name="Module">产生审计的模块</param>
/// <param name="Action">操作名</param>
/// <param name="Origin">请求来源，如 Http、ScheduledJob</param>
/// <param name="ActorUserId">操作人</param>
/// <param name="ActorName">操作人的名称；用户已经不存在时为空</param>
/// <param name="ActualUserId">模拟登录时真正动手的人</param>
/// <param name="ActualName">实际操作人的名称</param>
/// <param name="RoleId">操作时使用的角色</param>
/// <param name="RoleName">角色名称；角色已经不存在时为空</param>
/// <param name="SessionId">会话</param>
/// <param name="OnBehalfOfUserId">发起原始请求的用户</param>
/// <param name="ClientIp">客户端 IP</param>
/// <param name="TraceId">链路追踪 ID，可以到日志与链路里查这次操作</param>
/// <param name="Targets">操作对象</param>
/// <param name="Changes">字段变更：实体、变更类型、各字段新旧值，敏感字段已脱敏</param>
/// <param name="Details">补充说明，如导出的筛选条件</param>
public sealed record AuditDetail(
    Guid Id,
    DateTimeOffset OccurredAt,
    DateTimeOffset ReceivedAt,
    string Module,
    string Action,
    string Origin,
    Guid? ActorUserId,
    string? ActorName,
    Guid? ActualUserId,
    string? ActualName,
    Guid? RoleId,
    string? RoleName,
    Guid? SessionId,
    Guid? OnBehalfOfUserId,
    string? ClientIp,
    string TraceId,
    IReadOnlyList<AuditTargetItem> Targets,
    JsonElement Changes,
    JsonElement Details);
