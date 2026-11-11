namespace Kmic.BuildingBlocks.Infrastructure.SecurityEvents;

/// <summary>
/// 一条待写库的安全事件，身份、时间和追踪 ID 已经补全
/// </summary>
internal sealed record SecurityEventRecord
{
    public required Guid Id { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public required string Type { get; init; }

    public required short Severity { get; init; }

    public string? Reason { get; init; }

    public Guid? UserId { get; init; }

    public Guid? ActualUserId { get; init; }

    public Guid? RoleId { get; init; }

    public Guid? SessionId { get; init; }

    public string? AttemptedAccount { get; init; }

    public string? ResourceType { get; init; }

    public Guid? ResourceId { get; init; }

    public string? Details { get; init; }

    public required string TraceId { get; init; }
}
