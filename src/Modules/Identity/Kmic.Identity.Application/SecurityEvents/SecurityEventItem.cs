using Kmic.BuildingBlocks.Application.Security;

namespace Kmic.Identity.Application.SecurityEvents;

/// <summary>
/// 安全事件列表项
/// </summary>
/// <param name="Id">事件 ID</param>
/// <param name="OccurredAt">发生时间</param>
/// <param name="Type">类型</param>
/// <param name="Severity">严重程度</param>
/// <param name="Reason">原因编码</param>
/// <param name="UserId">用户 ID</param>
/// <param name="ActualUserId">实际操作人 ID</param>
/// <param name="RoleId">操作时使用的角色 ID</param>
/// <param name="SessionId">会话 ID</param>
/// <param name="AttemptedAccount">事件关联的账号</param>
/// <param name="ClientIp">客户端 IP</param>
/// <param name="ResourceType">资源类型</param>
/// <param name="ResourceId">资源 ID</param>
/// <param name="Details">补充说明</param>
/// <param name="TraceId">链路追踪 ID</param>
public sealed record SecurityEventItem(
    Guid Id,
    DateTimeOffset OccurredAt,
    string Type,
    SecuritySeverity Severity,
    string? Reason,
    Guid? UserId,
    Guid? ActualUserId,
    Guid? RoleId,
    Guid? SessionId,
    string? AttemptedAccount,
    string? ClientIp,
    string? ResourceType,
    Guid? ResourceId,
    IReadOnlyDictionary<string, string>? Details,
    string TraceId);
