using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 后台作业的处理器
/// </summary>
/// <remarks>
/// 不在事务里执行，只编排：通过 ISender 发送查询和命令，每个命令照常经过完整管道、各自一个事务。
/// 不注入 DbContext 和仓储。取消或失败时已经提交的批次保留
/// </remarks>
public interface IJobHandler<in TJob>
    where TJob : IBackgroundJob
{
    /// <summary>
    /// 执行作业
    /// </summary>
    /// <param name="job">作业参数</param>
    /// <param name="context">进度、检查点与批次提交</param>
    /// <param name="ct">用户取消、停机或超时时触发</param>
    Task<Result<JobOutcome>> ExecuteAsync(TJob job, IJobContext context, CancellationToken ct);
}
