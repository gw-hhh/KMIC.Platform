using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>作业执行时的进度、检查点与批次提交上下文。</summary>
public interface IJobContext
{
    Guid JobId { get; }

    /// <summary>取消原因，未取消时为空。</summary>
    JobCancellationReason? CancellationReason { get; }

    /// <summary>获取上次提交的检查点，首次执行时为空。</summary>
    T? GetCheckpoint<T>()
        where T : class;

    /// <summary>上报执行进度（最高每秒写入一次）；独立更新，不与业务事务绑定。</summary>
    ValueTask ReportProgressAsync(long done, long? total = null, string? message = null);

    /// <summary>批量发送命令，使业务数据、新检查点与进度在同一事务中提交；提交结果未知时禁止重试当前批次，应直接返回错误。</summary>
    ValueTask<Result<TResponse>> SendBatchAsync<TResponse>(ICommand<TResponse> command, object checkpoint, long done);

    /// <summary>批量发送无返回值命令，事务语义同上。</summary>
    ValueTask<Result> SendBatchAsync(ICommand command, object checkpoint, long done);
}
