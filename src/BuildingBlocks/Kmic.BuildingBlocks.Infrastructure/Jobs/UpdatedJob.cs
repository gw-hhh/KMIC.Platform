namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

internal sealed record UpdatedJob(
    short Status,
    string JobType,
    Guid? SubmittedBy,
    Guid? CancelRequestedBy,
    string? ErrorCode,
    string? ErrorMessage,
    Guid? ResultFileId,
    DateTimeOffset? FinishedAt);
