namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>快照事件（来源版本号位于消息信封中，不包含在事件载荷内）。</summary>
public interface ISnapshotEvent : IIntegrationEvent
{
    /// <summary>源数据 ID。</summary>
    Guid SourceId { get; }
}
