using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 作业执行时的进度、检查点与批次提交
/// </summary>
public interface IJobContext
{
    /// <summary>
    /// 作业 ID
    /// </summary>
    Guid JobId { get; }

    /// <summary>
    /// 取消的原因，没有取消时为空
    /// </summary>
    JobCancellationReason? CancellationReason { get; }

    /// <summary>
    /// 上次提交的检查点，从头开始时为空
    /// </summary>
    T? GetCheckpoint<T>()
        where T : class;

    /// <summary>
    /// 报告进度，最多每秒写一次
    /// </summary>
    /// <remarks>
    /// 不和业务数据一起提交，只读的作业（如导出）用它
    /// </remarks>
    ValueTask ReportProgressAsync(long done, long? total = null, string? message = null);

    /// <summary>
    /// 发送一批命令，命令的业务数据、新检查点和进度在同一个事务里提交
    /// </summary>
    /// <remarks>
    /// 提交结果未知时不要重试这一批，直接返回错误：作业会停下，按是否可续跑处理
    /// </remarks>
    ValueTask<Result<TResponse>> SendBatchAsync<TResponse>(ICommand<TResponse> command, object checkpoint, long done);

    /// <summary>
    /// 发送一批不返回值的命令，其余同上
    /// </summary>
    ValueTask<Result> SendBatchAsync(ICommand command, object checkpoint, long done);
}
