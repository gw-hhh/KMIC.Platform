namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

internal sealed record ClaimedInboxMessage
{
    public required Guid Id { get; init; }

    public required Guid MessageId { get; init; }

    public required string Subscription { get; init; }

    public required string MessageType { get; init; }

    public required int SchemaVersion { get; init; }

    public required string Payload { get; init; }

    public required string Headers { get; init; }

    public required string SourceModule { get; init; }

    public Guid? SourceId { get; init; }

    public long? SourceVersion { get; init; }

    public required DateTimeOffset OccurredAt { get; init; }

    public required int Attempts { get; init; }
}
