using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 后台作业的处理器
/// </summary>
/// <remarks>
/// 不在事务里执行，只通过 ISender 发查询和命令
/// </remarks>
public interface IJobHandler<in TJob>
    where TJob : IBackgroundJob
{
    /// <summary>
    /// 执行作业
    /// </summary>
    Task<Result<JobOutcome>> ExecuteAsync(TJob job, IJobContext context, CancellationToken ct);
}
