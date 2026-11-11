namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 收到的消息的信封
/// </summary>
/// <param name="MessageId">消息 ID，调用外部系统时作为幂等键</param>
/// <param name="MessageType">消息名称</param>
/// <param name="SchemaVersion">消息版本</param>
/// <param name="SourceModule">发布消息的模块</param>
/// <param name="OccurredAt">发生时间</param>
/// <param name="SourceId">快照对应的 ID，不是快照事件时为空</param>
/// <param name="SourceVersion">快照的来源版本，不是快照事件时为空</param>
/// <param name="Purged">快照事件：源数据已物理删除</param>
/// <param name="InitiatorUserId">发起这件事的用户</param>
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
