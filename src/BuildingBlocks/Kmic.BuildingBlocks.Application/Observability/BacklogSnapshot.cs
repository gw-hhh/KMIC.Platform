namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// 某一时刻的积压情况，由后台采样得到，指标回调直接读它，不在回调里查库
/// </summary>
/// <param name="Outbox">各模块的待投递消息</param>
/// <param name="Inbox">各订阅的待处理消息</param>
/// <param name="Jobs">各类后台作业的排队情况</param>
/// <param name="ScheduledJobs">各定时任务距离上次成功过了多久</param>
public sealed record BacklogSnapshot(
    IReadOnlyList<OutboxBacklog> Outbox,
    IReadOnlyList<InboxBacklog> Inbox,
    IReadOnlyList<JobBacklog> Jobs,
    IReadOnlyList<ScheduledJobFreshness> ScheduledJobs);
