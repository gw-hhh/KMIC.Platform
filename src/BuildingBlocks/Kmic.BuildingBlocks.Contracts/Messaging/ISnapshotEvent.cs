namespace Kmic.BuildingBlocks.Contracts.Messaging;

/// <summary>
/// 快照事件 {聚合}Changed：携带聚合对外公开的完整快照，读模型按来源版本取最新
/// </summary>
/// <remarks>
/// 来源版本不在载荷中，由保存时的拦截器取聚合递增后的版本写入信封
/// </remarks>
public interface ISnapshotEvent : IIntegrationEvent
{
    /// <summary>
    /// 快照所属聚合的 ID
    /// </summary>
    Guid SourceId { get; }
}
