namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

internal sealed record ClaimedJob
{
    public required Guid Id { get; init; }

    public required string JobType { get; init; }

    public required int SchemaVersion { get; init; }

    public required string Args { get; init; }

    public Guid? SubmittedBy { get; init; }

    public Guid? SubmittedRoleId { get; init; }

    // 领取前状态：运行中表示原执行器失联，取消中表示已请求取消
    public required short Previous { get; init; }

    public required int Attempts { get; init; }

    public string? Checkpoint { get; init; }
}
