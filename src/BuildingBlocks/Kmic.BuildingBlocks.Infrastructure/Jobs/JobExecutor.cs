using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Behaviors;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Contracts.Jobs;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Auditing;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
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
    IntegrationMessageCatalog messages,
    FrameworkMetrics metrics,
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

    /// <summary>
    /// 请求取消，作业已结束时返回 false
    /// </summary>
    /// <exception cref="CommitOutcomeUnknownException">提交没有得到确认</exception>
    public async Task<bool> RequestCancelAsync(string module, Guid jobId, UserId? by, CancellationToken ct) =>
        await UpdateAsync(module, jobId, db => store.RequestCancelAsync(db, module, jobId, by?.Value, Options.ResultRetention, ct)) is not null;

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "处理器的任何异常都只让这个作业失败，执行器继续处理别的作业，详细信息在日志里")]
    private async Task<JobRunResult> ExecuteAsync(
        string module, ClaimedJob job, Guid token, BackgroundJobType type, object args, (ICurrentUser User, IAccessContext? Access) identity, CancellationToken shutdown)
    {
        var started = TimeProvider.System.GetTimestamp();

        // 每次执行一个根 span，不挂在领取它的循环下面；每一批在它下面是一个子 span
        using var activity = KmicActivities.Messaging.StartActivity(
            $"job {type.Name}", ActivityKind.Internal, default(ActivityContext), [new("kmic.job.id", job.Id.ToString()), new("kmic.module", module)]);
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

            return await ReleaseAsync(module, job, token);
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

        if (finished is JobRunResult.Succeeded or JobRunResult.Failed or JobRunResult.Cancelled)
        {
            metrics.BackgroundJobFinished(module, type.Name, finished.ToString(), TimeProvider.System.GetElapsedTime(started));
        }

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
        try
        {
            var finished = await UpdateAsync(
                module,
                job.Id,
                db => store.FinishAsync(db, module, job.Id, token, status, error, outcome?.ResultFileId, summary, Options.ResultRetention, CancellationToken.None));
            return finished?.Status switch
            {
                null => JobRunResult.TakenOver,
                JobStatus.Succeeded => JobRunResult.Succeeded,
                JobStatus.Cancelled => JobRunResult.Cancelled,
                _ => JobRunResult.Failed,
            };
        }
        catch (CommitOutcomeUnknownException)
        {
            JobsLog.FinishOutcomeUnknown(logger, job.Id, job.JobType);
            return JobRunResult.OutcomeUnknown;
        }
    }

    private async Task<JobRunResult> ReleaseAsync(string module, ClaimedJob job, Guid token)
    {
        try
        {
            var released = await UpdateAsync(module, job.Id, db => store.ReleaseAsync(db, module, job.Id, token, Options.ResultRetention, CancellationToken.None));
            return released?.Status switch
            {
                null => JobRunResult.TakenOver,
                JobStatus.Cancelled => JobRunResult.Cancelled,
                _ => JobRunResult.Released,
            };
        }
        catch (CommitOutcomeUnknownException)
        {
            JobsLog.FinishOutcomeUnknown(logger, job.Id, job.JobType);
            return JobRunResult.OutcomeUnknown;
        }
    }

    // 作业行和结束事件在同一个事务里写：结果没写进去就不会发通知，同一个结果也只通知一次
    private async Task<UpdatedJob?> UpdateAsync(string module, Guid jobId, Func<ModuleDbContext, Task<UpdatedJob?>> update)
    {
        await using var scope = scopes.CreateScope(CallOrigin.BackgroundJob, CurrentUser.System(), null);
        var db = scope.ServiceProvider.GetRequiredService<ModuleWriteContexts>().Get(module);
        await using var transaction = await db.Database.BeginTransactionAsync(CancellationToken.None);
        var job = await update(db);
        if (job is null)
        {
            return null;
        }

        if (job is { SubmittedBy: { } submittedBy, FinishedAt: { } finishedAt } && ResultOf(job.Status) is { } result)
        {
            var finished = new BackgroundJobFinished
            {
                JobId = jobId,
                Module = module,
                JobType = job.JobType,
                JobName = catalog.DisplayNameOf(job.JobType),
                Result = result,
                ErrorCode = job.ErrorCode,
                ErrorMessage = job.ErrorMessage,
                ResultFileId = job.ResultFileId,
                SubmittedBy = submittedBy,
                CancelRequestedBy = job.CancelRequestedBy,
                FinishedAt = finishedAt,
            };
            using (db.Services.SaveGate.Open())
            {
                await scope.ServiceProvider.GetRequiredService<OutboxWriter>().WriteAsync(
                    db, module, [new PendingMessage(finished, messages.Describe(typeof(BackgroundJobFinished)))], CancellationToken.None);
            }
        }

        await CommandCommit.CommitAsync(transaction);
        return job;
    }

    private static BackgroundJobResult? ResultOf(short status) => status switch
    {
        JobStatus.Succeeded => BackgroundJobResult.Succeeded,
        JobStatus.Failed => BackgroundJobResult.Failed,
        JobStatus.Cancelled => BackgroundJobResult.Cancelled,
        _ => null,
    };
}
