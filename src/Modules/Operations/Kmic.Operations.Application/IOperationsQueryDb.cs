using Kmic.Operations.Application.Messages.Instances;
using Kmic.Operations.Application.Messages.Routes;
using Kmic.Operations.Application.ScheduledJobs;
using Kmic.Operations.Domain.Audit;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application;

/// <summary>Operations 模块查询入口。</summary>
public interface IOperationsQueryDb
{
    IQueryable<AuditLog> AuditLogs { get; }

    /// <summary>定时任务自定义开关与执行计划。</summary>
    IQueryable<ScheduledJobSetting> ScheduledJobSettings { get; }

    /// <summary>定时任务全部执行记录。</summary>
    IQueryable<ScheduledJobRunRow> ScheduledJobRuns { get; }

    /// <summary>各定时任务最近一次执行记录。</summary>
    IQueryable<ScheduledJobRunRow> LatestScheduledJobRuns { get; }

    /// <summary>订阅路由（消息类型向订阅方模块及订阅的投递关系）。</summary>
    IQueryable<MessageRouteRow> MessageRoutes { get; }

    /// <summary>实例及其订阅声明与最近心跳。</summary>
    IQueryable<MessagingInstanceRow> MessagingInstances { get; }

    /// <summary>按操作对象筛选审计记录。</summary>
    IQueryable<AuditLog> WithTarget(IQueryable<AuditLog> audits, string targetsJson);
}
