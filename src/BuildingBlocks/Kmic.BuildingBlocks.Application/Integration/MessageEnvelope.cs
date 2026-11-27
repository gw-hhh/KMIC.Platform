namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>集成消息信封元数据。</summary>
/// <param name="SourceId">快照对应的实体 ID；非快照事件时为 null。</param>
/// <param name="SourceVersion">快照来源版本号；非快照事件时为 null。</param>
/// <param name="Purged">是否为物理删除快照事件。</param>
public sealed record MessageEnvelope(
    Guid MessageId,
    string MessageType,
    int SchemaVersion,
    string SourceModule,
    DateTimeOffset OccurredAt,
    Guid? SourceId,
    long? SourceVersion,
    bool Purged,
    Guid? InitiatorUserId);
