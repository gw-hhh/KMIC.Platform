using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Auditing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

/// <summary>
/// 执行后台作业
/// </summary>
/// <remarks>
/// 以提交人在提交时角色下的身份执行（重新加载访问上下文），每一批各自一个事务。中断后按声明处理：
/// 可续跑的从检查点继续，其余标记失败，不会把已提交的批次再做一遍
/// </remarks>
internal sealed class JobExecutor(
    JobStore store,
    BackgroundJobCatalog catalog,
    IExecutionScopeFactory scopes,
    IEnumerable<IAccessContextLoader> loaders,
    IOptions<JobOptions> options,
    ILogger<JobExecutor> logger)
{
    private JobOptions Options => options.Value;

    private IAccessContextLoader Loader => loaders.FirstOrDefault()
        ?? throw new InvalidOperationException("没有注册 IAccessContextLoader，用户提交的后台作业无法执行。");

    public Task<IReadOnlyList<ClaimedJob>> ClaimAsync(string module, int max, Guid token, CancellationToken ct) =>
        store.ClaimAsync(module, max, token, Options.Lease, ct);

    /// <summary>
    /// 领取一批并依次执行完，返回领取到的个数
    /// </summary>
    public async Task<int> RunBatchAsync(string module, int max, CancellationToken ct)
    {
        var token = Guid.CreateVersion7();
        var claimed = await ClaimAsync(module, max, token, ct);
        foreach (var job in claimed)
        {
            await RunAsync(module, job, token, ct);
        }

        return claimed.Count;
    }

    /// <param name="module">作业所在模块</param>
    /// <param name="job">领取到的作业</param>
    /// <param name="token">本次领取的令牌</param>
    /// <param name="shutdown">停机时触发：可续跑的作业放回队列，其余标记中断</param>
    public async Task<JobRunResult> RunAsync(string module, ClaimedJob job, Guid token, CancellationToken shutdown)
    {
        if (job.Previous == JobStatus.Cancelling)
        {
            return await FinishAsync(module, job, token, JobStatus.Cancelled, null, null);
        }

        var type = catalog.Find(job.JobType, job.SchemaVersion);
        if (type is null)
        {
            return await FailAsync(module, job, token, CommonErrors.JobFailed.WithDetail($"本实例不认识作业 {job.JobType} 的版本 {job.SchemaVersion}。"));
        }

        // 执行者失联后领回来的：只有声明了可续跑、次数还没用完的才接着跑
        if (job.Previous == JobStatus.Running && (!type.Attribute.Resumable || job.Attempts > Options.MaxResumeAttempts + 1))
        {
            return await FailAsync(module, job, token, CommonErrors.JobInterrupted);
        }

        object args;
        try
        {
            args = JsonSerializer.Deserialize(job.Args, type.ClrType, KmicJsonOptions.Stored) ?? throw new JsonException("作业参数是 null。");
        }
        catch (JsonException exception)
        {
            return await FailAsync(module, job, token, CommonErrors.JobFailed.WithDetail($"作业参数读不出来：{exception.Message}"));
        }

        var identity = await LoadIdentityAsync(job, type, shutdown);
        if (identity is null)
        {
            return await FailAsync(module, job, token, CommonErrors.JobSubmitterUnavailable);
        }

        return await ExecuteAsync(module, job, token, type, args, identity.Value, shutdown);
    }

    public Task<bool> RequestCancelAsync(string module, Guid jobId, UserId? by, CancellationToken ct) =>
        store.RequestCancelAsync(module, jobId, by?.Value, Options.ResultRetention, ct);

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "处理器的任何异常都只让这个作业失败，执行器继续处理别的作业，详细信息在日志里")]
    private async Task<JobRunResult> ExecuteAsync(
        string module, ClaimedJob job, Guid token, BackgroundJobType type, object args, (ICurrentUser User, IAccessContext? Access) identity, CancellationToken shutdown)
    {
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(shutdown);
        var context = new JobContext(store, scopes, Options, module, job, token, identity.User, identity.Access, stop);
        var (result, failure) = await InvokeAsync(module, job, token, type, args, identity, context, stop);

        if (context.TakenOver)
        {
            return JobRunResult.TakenOver;
        }

        if (context.OutcomeUnknown)
        {
            JobsLog.BatchOutcomeUnknown(logger, job.Id, job.JobType);
            return JobRunResult.OutcomeUnknown;
        }

        if (shutdown.IsCancellationRequested)
        {
            if (!type.Attribute.Resumable)
            {
                return await FailAsync(module, job, token, CommonErrors.JobInterrupted);
            }

            return await store.ReleaseAsync(module, job.Id, token, CancellationToken.None) ? JobRunResult.Released : JobRunResult.TakenOver;
        }

        var finished = context.CancellationReason switch
        {
            JobCancellationReason.Timeout => await FailAsync(module, job, token, CommonErrors.JobTimedOut),
            JobCancellationReason.SubmitterDisabled => await FailAsync(module, job, token, CommonErrors.JobSubmitterUnavailable),
            JobCancellationReason.UserRequested => await FinishAsync(module, job, token, JobStatus.Cancelled, null, null),
            _ when failure is not null => await FailAsync(module, job, token, CommonErrors.JobFailed, failure),
            _ when result is { IsSuccess: true } => await FinishAsync(module, job, token, JobStatus.Succeeded, null, result.Value),
            _ => await FailAsync(module, job, token, result?.Error ?? CommonErrors.JobFailed),
        };

        if (type.Attribute.Audited && finished is JobRunResult.Succeeded or JobRunResult.Failed or JobRunResult.Cancelled)
        {
            await AuditAsync(module, job, type, identity, finished, result);
        }

        return finished;
    }


    // 调用处理器，同时按间隔续期占用。处理器的异常和取消都在这里收住，由调用方按原因决定作业的结局
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "处理器的任何异常都只让这个作业失败，执行器继续处理别的作业，详细信息在日志里")]
    private async Task<(Result<JobOutcome>? Result, Exception? Failure)> InvokeAsync(
        string module,
        ClaimedJob job,
        Guid token,
        BackgroundJobType type,
        object args,
        (ICurrentUser User, IAccessContext? Access) identity,
        JobContext context,
        CancellationTokenSource stop)
    {
        using var renewing = new CancellationTokenSource();
        var renewal = RenewAsync(module, job, token, context, identity.Access, renewing.Token);

        Result<JobOutcome>? result = null;
        Exception? failure = null;
        try
        {
            await using var scope = scopes.CreateScope(CallOrigin.BackgroundJob, identity.User, identity.Access);
            var handler = scope.ServiceProvider.GetRequiredService(type.HandlerInterface);
            var execute = type.HandlerInterface.GetMethod(nameof(IJobHandler<>.ExecuteAsync))!;
            result = await (Task<Result<JobOutcome>>)execute.Invoke(handler, [args, context, stop.Token])!;
        }
        catch (Exception exception) when (!stop.IsCancellationRequested)
        {
            failure = exception;
        }
        catch (Exception) when (stop.IsCancellationRequested)
        {
        }
        finally
        {
            await renewing.CancelAsync();
            await renewal;
        }

        return (result, failure);
    }

    // 作业结束后写一条审计事件，是尽力而为：作业的结果已经写好，审计写不进去只记日志
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "审计写不进去不能改变已经写好的作业结果，只记错误日志")]
    private async Task AuditAsync(
        string module, ClaimedJob job, BackgroundJobType type, (ICurrentUser User, IAccessContext? Access) identity, JobRunResult finished, Result<JobOutcome>? result)
    {
        try
        {
            await using var scope = scopes.CreateScope(CallOrigin.BackgroundJob, identity.User, identity.Access);
            var recorder = scope.ServiceProvider.GetRequiredService<AuditRecorder>();
            recorder.Add("jobId", job.Id.ToString("D"));
            recorder.Add("status", finished.ToString());
            recorder.Add("args", job.Args);
            if (result is { IsSuccess: true, Value.Summary: { } summary })
            {
                recorder.Add("summary", JsonSerializer.Serialize(summary, summary.GetType(), KmicJsonOptions.Stored));
            }

            await scope.ServiceProvider.GetRequiredService<ReadAuditWriter>().WriteAsync(recorder.BuildForJob(module, type.Name), module, CancellationToken.None);
        }
        catch (Exception exception)
        {
            JobsLog.AuditFailed(logger, exception, job.Id, job.JobType);
        }
    }
    // 每过占用时长的三分之一续期一次，顺便发现用户的取消请求、超时和提交人被禁用。
    // 占用以数据库时钟为准，这里按真实时间等待，不用注入的时钟（测试里它是固定的）
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "续期出错只记日志，占用到期前还有两次机会；到期后由别的实例接管")]
    private async Task RenewAsync(string module, ClaimedJob job, Guid token, JobContext context, IAccessContext? access, CancellationToken ct)
    {
        try
        {
            while (true)
            {
                await Task.Delay(Options.Lease / 3, TimeProvider.System, ct);
                try
                {
                    if (await store.RenewAsync(module, job.Id, token, Options.Lease, ct) is not { } renewed)
                    {
                        context.LoseOwnership();
                        return;
                    }

                    if (renewed.Elapsed > Options.MaxRunTime)
                    {
                        context.Cancel(JobCancellationReason.Timeout);
                    }
                    else if (renewed.Status == JobStatus.Cancelling)
                    {
                        context.Cancel(JobCancellationReason.UserRequested);
                    }
                    else if (access is not null && renewed.Elapsed > Options.SubmitterCheckAfter && !await SubmitterActiveAsync(access, ct))
                    {
                        context.Cancel(JobCancellationReason.SubmitterDisabled);
                    }
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    JobsLog.RenewFailed(logger, exception, job.Id);
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }
    }

    private async Task<(ICurrentUser, IAccessContext?)?> LoadIdentityAsync(ClaimedJob job, BackgroundJobType type, CancellationToken ct)
    {
        if (job.SubmittedBy is not { } submittedBy)
        {
            return (CurrentUser.System(), null);
        }

        var access = job.SubmittedRoleId is { } role ? await Loader.LoadForJobAsync(UserId.From(submittedBy), RoleId.From(role), ct) : null;
        return access is not null && access.Permissions.Contains(type.Attribute.Permission) ? (CurrentUser.From(access), access) : null;
    }

    // 只看人还在不在，不比较权限：一个作业从头到尾按开始时加载的权限执行
    private async Task<bool> SubmitterActiveAsync(IAccessContext access, CancellationToken ct) =>
        await Loader.LoadForJobAsync(access.UserId, access.RoleId, ct) is not null;

    private Task<JobRunResult> FailAsync(string module, ClaimedJob job, Guid token, Error error, Exception? exception = null)
    {
        JobsLog.JobFailed(logger, exception, job.Id, job.JobType, error.Code);
        return FinishAsync(module, job, token, JobStatus.Failed, error, null);
    }

    private async Task<JobRunResult> FinishAsync(string module, ClaimedJob job, Guid token, short status, Error? error, JobOutcome? outcome)
    {
        var summary = outcome?.Summary is { } value ? JsonSerializer.Serialize(value, value.GetType(), KmicJsonOptions.Stored) : null;
        var written = await store.FinishAsync(module, job.Id, token, status, error, outcome?.ResultFileId, summary, Options.ResultRetention, CancellationToken.None);
        if (!written)
        {
            return JobRunResult.TakenOver;
        }

        return status switch
        {
            JobStatus.Succeeded => JobRunResult.Succeeded,
            JobStatus.Cancelled => JobRunResult.Cancelled,
            _ => JobRunResult.Failed,
        };
    }
}
