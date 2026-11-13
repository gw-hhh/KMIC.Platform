using Kmic.Operations.Application.ScheduledJobs;
using Kmic.Operations.Domain.Audit;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application;

/// <summary>
/// Operations 模块的查询入口
/// </summary>
/// <remarks>
/// 查询处理器只从它出发：不跟踪实体，也不会读到命令里还没保存的修改。运维数据不声明数据范围，
/// 谁能看由权限点决定：审计人员需要看到全部
/// </remarks>
public interface IOperationsQueryDb
{
    /// <summary>
    /// 审计记录
    /// </summary>
    IQueryable<AuditLog> AuditLogs { get; }

    /// <summary>
    /// 运维改过的定时任务开关与执行时间
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
    /// 只留下操作对象里包含给定内容的审计记录
    /// </summary>
    /// <remarks>
    /// 操作对象存成 JSON 数组，按包含关系匹配，走表上的 GIN 索引。JSON 函数是数据库专有的，Application 不引用数据库驱动，所以放在查询入口里
    /// </remarks>
    /// <param name="audits">要筛选的审计记录</param>
    /// <param name="targetsJson">要包含的内容，如 [{"resourceType":"User","resourceId":"…"}]；对象里只写要匹配的属性</param>
    IQueryable<AuditLog> WithTarget(IQueryable<AuditLog> audits, string targetsJson);
}
