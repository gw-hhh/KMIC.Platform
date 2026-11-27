namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>后台作业状态视图（不含参数与检查点）。</summary>
public sealed record JobView(
    Guid Id,
    string Module,
    string JobType,
    string JobName,
    BackgroundJobState State,
    Guid? SubmittedBy,
    Guid? ActualUserId,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    long ProgressDone,
    long? ProgressTotal,
    string? ProgressMessage,
    Guid? ResultFileId,
    string? ResultSummary,
    string? ErrorCode,
    string? ErrorMessage,
    int Attempts,
    DateTimeOffset? CancelRequestedAt,
    Guid? CancelRequestedBy);
