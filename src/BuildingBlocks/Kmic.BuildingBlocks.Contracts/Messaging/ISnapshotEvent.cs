namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>
/// 快照事件
/// </summary>
/// <remarks>
/// 来源版本号不在事件里，在消息信封上
/// </remarks>
public interface ISnapshotEvent : IIntegrationEvent
{
    /// <summary>
    /// 快照对应的 ID
    /// </summary>
    Guid SourceId { get; }
}
