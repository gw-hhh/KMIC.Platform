using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>
/// 定时任务的运行控制：看调度库里的状态、把运维改过的开关与时间同步到触发器、马上执行一次
/// </summary>
/// <remarks>
/// 给 Operations 模块的管理命令用，其他模块不需要。Api 角色的调度器处于待机，也能用：
/// 改动写进共享的调度表，由运行着 Worker 的实例执行。运维改的开关与时间存在 Operations 的设置表里，
/// 这里只负责读它并同步，不负责写
/// </remarks>
public interface IScheduledJobControl
{
    /// <summary>
    /// 本实例有没有声明这个任务
    /// </summary>
    /// <param name="name">任务名</param>
    bool IsDeclared(string name);

    /// <summary>
    /// 本实例声明的全部定时任务（模块的和框架的），带调度库里的实际状态
    /// </summary>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlyList<ScheduledJobInfo>> ListAsync(CancellationToken ct);

    /// <summary>
    /// 把代码里的默认值和设置表里的覆盖值同步到触发器，整个集群即时生效
    /// </summary>
    /// <param name="ct">取消令牌</param>
    Task SyncAsync(CancellationToken ct);

    /// <summary>
    /// 马上执行一次，由运行着调度器的实例执行；执行记录里的触发方式是手动
    /// </summary>
    /// <param name="name">任务名</param>
    /// <param name="by">谁点的</param>
    /// <param name="ct">取消令牌</param>
    Task<ScheduledJobTriggerOutcome> TriggerAsync(string name, UserId by, CancellationToken ct);

    /// <summary>
    /// 检查一个 Cron 表达式并算出接下来几次触发的时间，按业务时区解释
    /// </summary>
    /// <param name="cron">Cron 表达式（Quartz 写法，带秒）</param>
    /// <param name="after">从这个时刻之后算起</param>
    /// <param name="count">要几次</param>
    CronPreview Preview(string cron, DateTimeOffset after, int count);
}
