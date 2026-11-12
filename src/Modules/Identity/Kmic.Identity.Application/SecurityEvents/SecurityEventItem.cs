using Kmic.BuildingBlocks.Application.Security;

namespace Kmic.Identity.Application.SecurityEvents;

/// <summary>
/// 安全事件列表里的一行
/// </summary>
/// <param name="Id">事件 ID</param>
/// <param name="OccurredAt">发生时间</param>
/// <param name="Type">类型</param>
/// <param name="Severity">严重程度</param>
/// <param name="Reason">原因编码</param>
/// <param name="UserId">用户</param>
/// <param name="ActualUserId">模拟身份时实际操作的人</param>
/// <param name="RoleId">当时的角色</param>
/// <param name="SessionId">会话</param>
/// <param name="AttemptedAccount">登录失败时输入的账号</param>
/// <param name="ClientIp">客户端 IP</param>
/// <param name="ResourceType">资源类型</param>
/// <param name="ResourceId">资源 ID</param>
/// <param name="Details">补充说明</param>
/// <param name="TraceId">链路追踪 ID，可以到日志与链路里查这次请求</param>
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
