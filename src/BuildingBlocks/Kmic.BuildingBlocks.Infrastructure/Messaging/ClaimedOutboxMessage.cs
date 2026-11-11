namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 投递程序领取到的一条 Outbox 消息
/// </summary>
internal sealed record ClaimedOutboxMessage
{
    public required Guid Id { get; init; }

    public required long Seq { get; init; }

    public required string MessageType { get; init; }

    public required int SchemaVersion { get; init; }

    public required string Payload { get; init; }

    public required string Headers { get; init; }

    public Guid? SourceId { get; init; }

    public long? SourceVersion { get; init; }

    public string? OrderingKey { get; init; }

    public Guid[]? SubjectIds { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public required int Attempts { get; init; }
}
