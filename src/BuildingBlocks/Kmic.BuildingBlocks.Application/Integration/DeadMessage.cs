namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>死信记录，表示投递或处理持续失败、等待运维重试或放弃的消息。</summary>
public sealed record DeadMessage(
    Guid Id,
    MessageBox Box,
    string Module,
    string? Subscription,
    Guid MessageId,
    string MessageType,
    int SchemaVersion,
    DateTimeOffset OccurredAt,
    int Attempts,
    string? LastError);
