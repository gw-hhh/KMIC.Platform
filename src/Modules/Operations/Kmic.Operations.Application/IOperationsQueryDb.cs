using Kmic.Operations.Application.Messages.Instances;
using Kmic.Operations.Application.Messages.Routes;
using Kmic.Operations.Application.ScheduledJobs;
using Kmic.Operations.Domain.Audit;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application;

/// <summary>
/// Operations 模块的查询入口，不跟踪实体，只读已提交的数据
/// </summary>
/// <remarks>
/// 运维数据不声明数据范围，能看什么由用例的权限点决定
/// </remarks>
public interface IOperationsQueryDb
{
    /// <summary>
    /// 审计记录
    /// </summary>
    IQueryable<AuditLog> AuditLogs { get; }

    /// <summary>
    /// 定时任务的自定义开关和执行时间
    /// </summary>
    IQueryable<ScheduledJobSetting> ScheduledJobSettings { get; }

    /// <summary>
    /// 定时任务的全部执行记录
    /// </summary>
    IQueryable<ScheduledJobRunRow> ScheduledJobRuns { get; }

    /// <summary>
    /// 每个定时任务最近一次的执行记录
    /// </summary>
    IQueryable<ScheduledJobRunRow> LatestScheduledJobRuns { get; }

    /// <summary>
    /// 订阅路由：哪种消息投递给哪个模块的哪个订阅
    /// </summary>
    IQueryable<MessageRouteRow> MessageRoutes { get; }

    /// <summary>
    /// 实例及其订阅声明和最近心跳
    /// </summary>
    IQueryable<MessagingInstanceRow> MessagingInstances { get; }

    /// <summary>
    /// 按操作对象筛选审计记录
    /// </summary>
    /// <remarks>
    /// 按 JSON 包含匹配，走 GIN 索引；数据库函数放在实现层，Application 不依赖数据库驱动
    /// </remarks>
    /// <param name="audits">要筛选的审计记录</param>
    /// <param name="targetsJson">操作对象的 JSON 匹配条件，如 [{"resourceType":"User","resourceId":"…"}]；可只提供要匹配的属性</param>
    IQueryable<AuditLog> WithTarget(IQueryable<AuditLog> audits, string targetsJson);
}
