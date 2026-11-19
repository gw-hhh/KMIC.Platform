using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>
/// 定时任务的运行控制，给 Operations 的管理命令用
/// </summary>
public interface IScheduledJobControl
{
    /// <summary>
    /// 本实例有没有声明这个任务
    /// </summary>
    bool IsDeclared(string name);

    /// <summary>
    /// 本实例声明的全部定时任务（模块的和框架的），带调度库里的实际状态
    /// </summary>
    Task<IReadOnlyList<ScheduledJobInfo>> ListAsync(CancellationToken ct);

    /// <summary>
    /// 把代码里的默认值和设置表里的覆盖值同步到触发器，整个集群即时生效
    /// </summary>
    Task SyncAsync(CancellationToken ct);

    /// <summary>
    /// 马上执行一次，由运行着调度器的实例执行；任务忙或已有手动触发在排队时拒绝，执行记录里的触发方式是手动
    /// </summary>
    Task<ScheduledJobTriggerOutcome> TriggerAsync(string name, UserId by, CancellationToken ct);

    /// <summary>
    /// 检查一个 Cron 表达式并算出接下来几次触发的时间，按业务时区解释
    /// </summary>
    CronPreview Preview(string cron, DateTimeOffset after, int count);
}
