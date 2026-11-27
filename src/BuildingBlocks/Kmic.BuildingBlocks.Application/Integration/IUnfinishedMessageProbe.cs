namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>未完成消息探针，用于检查指定对象是否仍被在途消息引用；仅限在命令处理器中调用。</summary>
public interface IUnfinishedMessageProbe
{
    /// <summary>查询仍被指定类型未完成消息引用的对象 ID 集合。</summary>
    Task<IReadOnlySet<Guid>> FindSubjectsAsync(string messageType, IReadOnlyCollection<Guid> subjectIds, CancellationToken ct);
}
