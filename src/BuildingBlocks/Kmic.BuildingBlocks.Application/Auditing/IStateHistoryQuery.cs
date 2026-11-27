using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.BuildingBlocks.Application.Auditing;

/// <summary>业务状态历史查询；本接口不校验权限，调用方须先确认当前用户有权访问目标数据。</summary>
public interface IStateHistoryQuery
{
    /// <summary>查询实体的全部状态流转记录，按时间排序。</summary>
    Task<IReadOnlyList<StateHistoryItem>> GetAsync<TAggregate>(Guid aggregateId, CancellationToken ct)
        where TAggregate : IAggregateRoot;
}
