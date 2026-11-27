using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Jobs;

public interface IModuleJobStore
{
    string Module { get; }

    /// <summary>按提交时间倒序分页获取作业，时间相同时按 ID 倒序。</summary>
    Task<IReadOnlyList<JobView>> ListAsync(JobFilter filter, JobPosition? after, int take, CancellationToken ct);

    /// <summary>按 ID 获取作业，不存在时返回 null。</summary>
    Task<JobView?> FindAsync(Guid id, CancellationToken ct);

    /// <summary>请求取消作业；作业已结束或不存在时返回 false。</summary>
    Task<bool> RequestCancelAsync(Guid id, UserId by, CancellationToken ct);
}
