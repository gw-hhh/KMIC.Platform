using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.BuildingBlocks.Application.Auditing;

/// <summary>
/// 业务历史查询
/// </summary>
/// <remarks>
/// 不检查权限，处理器要先确认能看这条数据
/// </remarks>
public interface IStateHistoryQuery
{
    /// <summary>
    /// 全部状态流转，按时间排序
    /// </summary>
    Task<IReadOnlyList<StateHistoryItem>> GetAsync<TAggregate>(Guid aggregateId, CancellationToken ct)
        where TAggregate : IAggregateRoot;
}
