using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>后台作业处理器；不在事务中执行，须通过 ISender 发送查询与命令。</summary>
public interface IJobHandler<in TJob>
    where TJob : IBackgroundJob
{
    Task<Result<JobOutcome>> ExecuteAsync(TJob job, IJobContext context, CancellationToken ct);
}
