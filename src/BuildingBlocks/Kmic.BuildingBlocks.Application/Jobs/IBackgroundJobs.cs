using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>后台作业提交入口，仅可在命令中调用，与业务数据在同一事务中提交。</summary>
public interface IBackgroundJobs
{
    /// <summary>将后台作业加入队列；同用户同类作业数达上限时返回 Common.TooManyJobs。</summary>
    Task<Result<JobAccepted>> EnqueueAsync(IBackgroundJob job, CancellationToken ct);
}
