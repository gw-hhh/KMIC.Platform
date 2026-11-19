using Kmic.Operations.Application.Messages.Instances;
using Kmic.Operations.Application.Messages.Routes;
using Kmic.Operations.Application.ScheduledJobs;
using Kmic.Operations.Domain.Audit;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application;

/// <summary>
/// Operations 模块的查询入口
/// </summary>
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
    IQueryable<AuditLog> WithTarget(IQueryable<AuditLog> audits, string targetsJson);
}
