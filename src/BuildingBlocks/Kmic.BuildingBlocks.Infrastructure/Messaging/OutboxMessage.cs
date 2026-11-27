namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

internal sealed class OutboxMessage
{
    public Guid Id { get; set; }

    public long Seq { get; set; }

    public string MessageType { get; set; } = "";

    public int SchemaVersion { get; set; }

    // 0 事件，1 请求
    public short Kind { get; set; }

    public string Payload { get; set; } = "";

    public string Headers { get; set; } = "";

    // 快照对应的实体 ID 与版本号
    public Guid? SourceId { get; set; }

    public long? SourceVersion { get; set; }

    public string? OrderingKey { get; set; }

    public Guid[]? SubjectIds { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public short Status { get; set; }

    public int Attempts { get; set; }

    public DateTimeOffset NextAttemptAt { get; set; }

    public Guid? LeaseToken { get; set; }

    public DateTimeOffset? LeaseUntil { get; set; }

    public string? LastError { get; set; }

    public DateTimeOffset? DeliveredAt { get; set; }
}
