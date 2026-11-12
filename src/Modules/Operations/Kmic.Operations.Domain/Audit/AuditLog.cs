using System.Net;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Operations.Domain.Audit;

/// <summary>
/// 一条审计记录：谁在什么时候、以什么方式、对什么对象做了什么，字段怎么变的
/// </summary>
/// <remarks>
/// 只插入，不改不删：表上对运行账号撤销了修改与删除权限，保留期到期的数据只能由迁移程序的维护命令删除。
/// 不是聚合行为，是把审计消息落成一行。写入这一行的时间是创建时间，和发生时间 <see cref="OccurredAt" /> 不同
/// </remarks>
public sealed class AuditLog : AggregateRoot<AuditLogId>
{
    private AuditLog(AuditLogId id)
        : base(id)
    {
    }

    /// <summary>
    /// 操作发生的时间
    /// </summary>
    public DateTimeOffset OccurredAt { get; private set; }

    /// <summary>
    /// 产生这条审计的模块
    /// </summary>
    public string Module { get; private set; } = string.Empty;

    /// <summary>
    /// 操作名，如 Identity.CreateUser
    /// </summary>
    public string Action { get; private set; } = string.Empty;

    /// <summary>
    /// 请求来源，如 Http、ScheduledJob
    /// </summary>
    public string Origin { get; private set; } = string.Empty;

    /// <summary>
    /// 操作人；系统身份与匿名请求没有
    /// </summary>
    public UserId? ActorUserId { get; private set; }

    /// <summary>
    /// 模拟登录时真正动手的人
    /// </summary>
    public UserId? ActualUserId { get; private set; }

    /// <summary>
    /// 操作时使用的角色
    /// </summary>
    public RoleId? RoleId { get; private set; }

    /// <summary>
    /// 会话
    /// </summary>
    public SessionId? SessionId { get; private set; }

    /// <summary>
    /// 发起原始请求的用户：事件处理器代表他执行时记录
    /// </summary>
    public UserId? OnBehalfOfUserId { get; private set; }

    /// <summary>
    /// 客户端 IP
    /// </summary>
    public IPAddress? ClientIp { get; private set; }

    /// <summary>
    /// 链路追踪 ID，与错误响应和日志中的一致
    /// </summary>
    public string TraceId { get; private set; } = string.Empty;

    /// <summary>
    /// 操作对象，JSON 数组：资源类型与 ID
    /// </summary>
    public string Targets { get; private set; } = "[]";

    /// <summary>
    /// 字段变更，JSON 数组：实体、变更类型、各字段新旧值（敏感字段已脱敏）
    /// </summary>
    public string Changes { get; private set; } = "[]";

    /// <summary>
    /// 补充说明，JSON 对象，如导出的筛选条件
    /// </summary>
    public string Details { get; private set; } = "{}";

    /// <summary>
    /// 把一条审计消息落成一行
    /// </summary>
    public static AuditLog Record(
        AuditLogId id,
        DateTimeOffset occurredAt,
        string module,
        string action,
        string origin,
        string traceId,
        string targets,
        string changes,
        string details,
        UserId? actorUserId = null,
        UserId? actualUserId = null,
        RoleId? roleId = null,
        SessionId? sessionId = null,
        UserId? onBehalfOfUserId = null,
        IPAddress? clientIp = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(module);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(origin);
        ArgumentNullException.ThrowIfNull(traceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targets);
        ArgumentException.ThrowIfNullOrWhiteSpace(changes);
        ArgumentException.ThrowIfNullOrWhiteSpace(details);

        return new AuditLog(id)
        {
            OccurredAt = occurredAt,
            Module = module,
            Action = action,
            Origin = origin,
            ActorUserId = actorUserId,
            ActualUserId = actualUserId,
            RoleId = roleId,
            SessionId = sessionId,
            OnBehalfOfUserId = onBehalfOfUserId,
            ClientIp = clientIp,
            TraceId = traceId,
            Targets = targets,
            Changes = changes,
            Details = details,
        };
    }
}
