namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// 消息信封扩展字段（Purged 表示快照事件）
internal sealed record MessageHeaders(
    string SourceModule,
    Guid? InitiatorUserId,
    Guid? ActualUserId,
    Guid? CausationId,
    string? TraceParent,
    string? TraceState,
    bool Purged);
