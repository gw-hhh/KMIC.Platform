namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// 执行器领取到的一个作业
/// </summary>
internal sealed record ClaimedJob
{
    public required Guid Id { get; init; }

    public required string JobType { get; init; }

    public required int SchemaVersion { get; init; }

    public required string Args { get; init; }

    public Guid? SubmittedBy { get; init; }

    public Guid? SubmittedRoleId { get; init; }

    // 领取前的状态：运行中说明上一个执行者失联了，取消中说明用户已经要求取消
    public required short Previous { get; init; }

    public required int Attempts { get; init; }

    public string? Checkpoint { get; init; }
}
