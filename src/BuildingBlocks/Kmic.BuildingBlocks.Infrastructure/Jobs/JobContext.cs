using System.Text.Json;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// 一次作业执行的进度、检查点与批次提交
/// </summary>
/// <remarks>
/// 每一批在新的作用域里以提交人的身份发送，照常经过完整管道；检查点作为提交前的步骤带令牌写入，和业务数据一起提交
/// </remarks>
internal sealed class JobContext(
    JobStore store,
    IExecutionScopeFactory scopes,
    JobOptions options,
    string module,
    ClaimedJob job,
    Guid token,
    ICurrentUser user,
    IAccessContext? access,
    CancellationTokenSource stop) : IJobContext
{
    private string? _checkpoint = job.Checkpoint;
    private long _lastProgress = long.MinValue;

    public Guid JobId => job.Id;

    public JobCancellationReason? CancellationReason { get; private set; }

    // 占用已被别的执行者接管：本地停下，不再写任何结果
    public bool TakenOver { get; private set; }

    // 某一批的提交结果未知：本地停下，不重试这一批，由领取流程按是否可续跑处理
    public bool OutcomeUnknown { get; private set; }

    public T? GetCheckpoint<T>()
        where T : class =>
        _checkpoint is null ? null : JsonSerializer.Deserialize<T>(_checkpoint, KmicJsonOptions.Stored);

    public async ValueTask ReportProgressAsync(long done, long? total = null, string? message = null)
    {
        // 节流只看真实经过的时间
        var now = TimeProvider.System.GetTimestamp();
        if (_lastProgress != long.MinValue && TimeProvider.System.GetElapsedTime(_lastProgress, now) < options.ProgressInterval)
        {
            return;
        }

        _lastProgress = now;
        if (!await store.ReportProgressAsync(module, job.Id, token, done, total, message, stop.Token))
        {
            LoseOwnership();
        }
    }

    public async ValueTask<Result<TResponse>> SendBatchAsync<TResponse>(ICommand<TResponse> command, object checkpoint, long done)
    {
        ArgumentNullException.ThrowIfNull(command);
        var (scope, json) = Prepare(checkpoint, done);
        await using (scope)
        {
            return Settle(await scope.ServiceProvider.GetRequiredService<ISender>().SendAsync(command, stop.Token), json);
        }
    }

    public async ValueTask<Result> SendBatchAsync(ICommand command, object checkpoint, long done)
    {
        ArgumentNullException.ThrowIfNull(command);
        var (scope, json) = Prepare(checkpoint, done);
        await using (scope)
        {
            return Settle(await scope.ServiceProvider.GetRequiredService<ISender>().SendAsync(command, stop.Token), json);
        }
    }

    public void Cancel(JobCancellationReason reason)
    {
        CancellationReason ??= reason;
        stop.Cancel();
    }

    public void LoseOwnership()
    {
        TakenOver = true;
        stop.Cancel();
    }

    private (AsyncServiceScope Scope, string Checkpoint) Prepare(object checkpoint, long done)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        var json = JsonSerializer.Serialize(checkpoint, checkpoint.GetType(), KmicJsonOptions.Stored);
        var scope = scopes.CreateScope(CallOrigin.BackgroundJob, user, access);
        scope.ServiceProvider.GetRequiredService<CommandTransaction>().BeforeCommit(async (db, _, ct) =>
        {
            if (await store.SaveCheckpointAsync(db, module, job.Id, token, json, done, ct))
            {
                return null;
            }

            TakenOver = true;
            return CommonErrors.RequestInProgress;
        });
        return (scope, json);
    }

    private TResult Settle<TResult>(TResult result, string checkpoint)
        where TResult : Result
    {
        if (result.IsSuccess)
        {
            _checkpoint = checkpoint;
        }
        else if (TakenOver)
        {
            stop.Cancel();
        }
        else if (string.Equals(result.Error.Code, CommonErrors.ResultUnknown.Code, StringComparison.Ordinal))
        {
            OutcomeUnknown = true;
            stop.Cancel();
        }

        return result;
    }
}
