using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Scheduling;

public interface IScheduledJobControl
{
    /// <summary>检查当前实例是否已声明指定任务。</summary>
    bool IsDeclared(string name);

    /// <summary>列出当前实例声明的所有定时任务及其调度状态。</summary>
    Task<IReadOnlyList<ScheduledJobInfo>> ListAsync(CancellationToken ct);

    /// <summary>将代码默认值与设置表覆盖值同步至集群触发器。</summary>
    Task SyncAsync(CancellationToken ct);

    /// <summary>手动触发一次定时任务；任务运行中或已有触发排队时拒绝。</summary>
    Task<ScheduledJobTriggerOutcome> TriggerAsync(string name, UserId by, CancellationToken ct);

    /// <summary>校验 Cron 表达式并按业务时区计算后续触发时间。</summary>
    CronPreview Preview(string cron, DateTimeOffset after, int count);
}
