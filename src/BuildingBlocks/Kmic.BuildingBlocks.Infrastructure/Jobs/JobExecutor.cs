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
        ?? throw new InvalidOperationException("未注册 IAccessContextLoader，无法执行用户提交的后台作业。");

    public Task<IReadOnlyList<ClaimedJob>> ClaimAsync(string module, int max, Guid token, CancellationToken ct) =>
        store.ClaimAsync(module, max, token, Options.Lease, ct);

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
            return await FailAsync(module, job, token, CommonErrors.JobFailed.WithDetail($"当前实例无法识别作业 {job.JobType} 的版本 {job.SchemaVersion}。"));
        }

        // 原执行器失联回收的作业：仅可续跑且未达重试上限时继续执行
        if (job.Previous == JobStatus.Running && (!type.Attribute.Resumable || job.Attempts > Options.MaxResumeAttempts + 1))
        {
            return await FailAsync(module, job, token, CommonErrors.JobInterrupted);
        }

        object args;
        try
        {
            args = JsonSerializer.Deserialize(job.Args, type.ClrType, KmicJsonOptions.Stored) ?? throw new JsonException("作业参数反序列化结果为空。");
        }
        catch (JsonException exception)
        {
            return await FailAsync(module, job, token, CommonErrors.JobFailed.WithDetail($"作业参数反序列化失败：{exception.Message}"));
        }

        var identity = await LoadIdentityAsync(job, type, shutdown);
        if (identity is null)
        {
            return await FailAsync(module, job, token, CommonErrors.JobSubmitterUnavailable);
        }

        return await ExecuteAsync(module, job, token, type, args, identity.Value, shutdown);
    }

    // 请求取消作业，已结束返回 false；提交结果未确认时抛出 CommitOutcomeUnknownException
    public async Task<bool> RequestCancelAsync(string module, Guid jobId, UserId? by, CancellationToken ct) =>
        await UpdateAsync(module, jobId, db => store.RequestCancelAsync(db, module, jobId, by?.Value, Options.ResultRetention, ct)) is not null;

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "单次作业处理器异常仅标记当前作业失败并记录详细日志，不中断执行器继续处理后续作业")]
    private async Task<JobRunResult> ExecuteAsync(
        string module, ClaimedJob job, Guid token, BackgroundJobType type, object args, (ICurrentUser User, IAccessContext? Access) identity, CancellationToken shutdown)
    {
        var started = TimeProvider.System.GetTimestamp();

        // 每次作业执行创建独立的根 span（不挂在领取循环下），各批次作为其子 span
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


    // 调用作业处理器并定时续期占用，捕获所有处理器异常与取消信号，由调用方统一判定最终状态
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "单次作业处理器异常仅标记当前作业失败并记录详细日志，不中断执行器继续处理后续作业")]
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

    // 作业结束后尽力写入审计事件，写入失败仅记录日志
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "审计事件写入失败不应改变已提交的作业结果，仅记录错误日志")]
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
    // 每隔占用时长的 1/3 续期一次，并同步检测取消请求、超时及提交人状态
    // 占用过期以数据库时钟为准，此处按真实物理时间等待，避免使用测试中可能被固定的注入时钟
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "占用续期异常仅记录日志，占用过期前仍有两次重试机会，过期后将由其他实例接管")]
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

    // 仅校验提交人是否仍有效，不复核权限变更，作业全程沿用启动时加载的权限快照
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

    // 作业状态与结束事件在同一事务中写入，确保结果落库前不发通知且同一结果仅通知一次
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
                    db, module, [new PendingMessage(finished, messages.Describe(typeof(BackgroundJobFinished)))], AdmissionMode.SkipWhenSuspended, CancellationToken.None);
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
