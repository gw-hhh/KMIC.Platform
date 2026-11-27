namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

// 后台作业表持久化记录（仅用于建表与提交）
internal sealed class BackgroundJobRecord
{
    public Guid Id { get; set; }

    public string JobType { get; set; } = "";

    public int SchemaVersion { get; set; }

    public string Args { get; set; } = "";

    // 系统提交时为空
    public Guid? SubmittedBy { get; set; }

    public Guid? SubmittedRoleId { get; set; }

    // 模拟登录时的实际操作人
    public Guid? ActualUserId { get; set; }

    public DateTimeOffset SubmittedAt { get; set; }

    public short Status { get; set; }

    public long ProgressDone { get; set; }

    public long? ProgressTotal { get; set; }

    public string? ProgressMessage { get; set; }

    public string? Checkpoint { get; set; }

    public Guid? ResultFileId { get; set; }

    public string? ResultSummary { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public int Attempts { get; set; }

    public Guid? LeaseToken { get; set; }

    public DateTimeOffset? LeaseUntil { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }

    public DateTimeOffset? CancelRequestedAt { get; set; }

    public Guid? CancelRequestedBy { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }
}
