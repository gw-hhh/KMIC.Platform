using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 一个模块的后台作业存储，给 Operations 的管理视图用
/// </summary>
public interface IModuleJobStore
{
    /// <summary>
    /// 模块名
    /// </summary>
    string Module { get; }

    /// <summary>
    /// 按提交时间倒序取一批，时间相同按 ID 倒序
    /// </summary>
    Task<IReadOnlyList<JobView>> ListAsync(JobFilter filter, JobPosition? after, int take, CancellationToken ct);

    /// <summary>
    /// 按 ID 取一个作业，没有时返回空
    /// </summary>
    Task<JobView?> FindAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// 请求取消：排队中的直接取消，运行中的改为取消中，执行器在下次续期时停下。取消不是回滚，已提交的批次保留
    /// </summary>
    /// <returns>作业已经结束（或不存在）时返回 false</returns>
    Task<bool> RequestCancelAsync(Guid id, UserId by, CancellationToken ct);
}
