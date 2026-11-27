namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>后台采样的积压快照，供指标回调直接读取。</summary>
public sealed record BacklogSnapshot(
    IReadOnlyList<OutboxBacklog> Outbox,
    IReadOnlyList<InboxBacklog> Inbox,
    IReadOnlyList<JobBacklog> Jobs,
    IReadOnlyList<ScheduledJobFreshness> ScheduledJobs);
