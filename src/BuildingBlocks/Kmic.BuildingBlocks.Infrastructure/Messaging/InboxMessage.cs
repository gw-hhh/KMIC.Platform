namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// Inbox 消息表记录（每行对应一条消息的一个订阅）
internal sealed class InboxMessage
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public string Subscription { get; set; } = "";

    public string MessageType { get; set; } = "";

    public int SchemaVersion { get; set; }

    public string Payload { get; set; } = "";

    public string Headers { get; set; } = "";

    public string SourceModule { get; set; } = "";

    // 快照对应的实体 ID 与版本号
    public Guid? SourceId { get; set; }

    public long? SourceVersion { get; set; }

    public string? OrderingKey { get; set; }

    public long Seq { get; set; }

    public Guid[]? SubjectIds { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public DateTimeOffset ReceivedAt { get; set; }

    public short Status { get; set; }

    public int Attempts { get; set; }

    public DateTimeOffset NextAttemptAt { get; set; }

    public Guid? LeaseToken { get; set; }

    public DateTimeOffset? LeaseUntil { get; set; }

    public string? LastError { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }
}
