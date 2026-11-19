using Kmic.BuildingBlocks.Application.Scheduling;

namespace Kmic.BuildingBlocks.Infrastructure.Scheduling;

/// <summary>
/// 框架自带的定时任务，如清理过期的框架表数据
/// </summary>
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
