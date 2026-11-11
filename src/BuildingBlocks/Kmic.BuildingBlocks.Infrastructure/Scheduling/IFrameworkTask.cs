using Kmic.BuildingBlocks.Application.Scheduling;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 框架自带的定时任务，如清理过期的框架表数据
/// </summary>
/// <remarks>
/// 不是命令：它要遍历所有模块的框架表，每批一个短事务，不属于哪个模块，也不需要命令管道的授权和审计。
/// 调度、执行记录、开关和改时间与模块的定时任务完全一样
/// </remarks>
internal interface IFrameworkTask
{
    /// <summary>
    /// 稳定名称，以 "Framework." 开头
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 默认执行时间，按业务时区解释
    /// </summary>
    string Cron { get; }

    string DisplayName { get; }

    Task<ScheduledJobOutcome> ExecuteAsync(CancellationToken ct);
}
