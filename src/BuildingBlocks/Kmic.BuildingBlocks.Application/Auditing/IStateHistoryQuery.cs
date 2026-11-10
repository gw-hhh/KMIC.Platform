using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.BuildingBlocks.Application.Auditing;

/// <summary>
/// 查询聚合的业务历史
/// </summary>
/// <remarks>
/// 历史不单独授权：处理器先按正常方式加载资源（数据范围、资源策略生效），能看到资源才调用这里
/// </remarks>
public interface IStateHistoryQuery
{
    /// <summary>
    /// 按发生时间排列的全部流转
    /// </summary>
    Task<IReadOnlyList<StateHistoryItem>> GetAsync<TAggregate>(Guid aggregateId, CancellationToken cancellationToken)
        where TAggregate : IAggregateRoot;
}
