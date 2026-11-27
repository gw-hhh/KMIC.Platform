using System.Text.Json;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Serialization;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Messaging;
using Kmic.BuildingBlocks.Infrastructure.Pipeline;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Jobs;

// 后台作业提交器：写入当前模块的写入上下文并随命令事务提交
internal sealed class BackgroundJobs(
    BackgroundJobCatalog catalog,
    IExecutionContext execution,
    ModuleWriteContexts contexts,
    CommandTransaction transaction,
    TimeProvider time) : IBackgroundJobs
{
    public async Task<Result<JobAccepted>> EnqueueAsync(IBackgroundJob job, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (!execution.InCommand)
        {
            throw new InvalidOperationException("后台作业仅限在命令中提交，须与业务数据在同一事务中落库。");
        }

        var type = catalog.Describe(job.GetType());

        // 仅保存当前命令所属模块的写入上下文，跨模块写入无法随事务落库
        if (!string.Equals(transaction.Module, type.Module, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"作业 {type.Name} 属于 {type.Module} 模块，只能由该模块的命令提交。");
        }

        var user = execution.User;
        if (type.Attribute.SystemOnly && !user.IsSystem)
        {
            throw new InvalidOperationException($"作业 {type.Name} 只能由系统身份提交。");
        }

        // 提交命令自身已声明权限点，此处再校验一次作业所需权限点，执行阶段亦会复核
        if (!user.IsSystem && execution.Access?.Permissions.Contains(type.Attribute.Permission) != true)
        {
            return CommonErrors.MissingPermission;
        }

        var jobs = contexts.Get(type.Module).Set<BackgroundJobRecord>();
        if (user.UserId is { } userId)
        {
            var unfinished = await jobs.CountAsync(
                j => j.SubmittedBy == userId.Value && j.JobType == type.Name
                    && (j.Status == JobStatus.Queued || j.Status == JobStatus.Running || j.Status == JobStatus.Cancelling),
                ct);
            if (unfinished >= type.Attribute.MaxConcurrentPerUser)
            {
                return CommonErrors.TooManyJobs;
            }
        }

        var record = new BackgroundJobRecord
        {
            Id = Guid.CreateVersion7(),
            JobType = type.Name,
            SchemaVersion = type.Version,
            Args = JsonSerializer.Serialize(job, type.ClrType, KmicJsonOptions.Stored),
            SubmittedBy = user.UserId?.Value,
            SubmittedRoleId = user.RoleId?.Value,
            ActualUserId = user.ActualUserId?.Value,
            SubmittedAt = time.GetUtcNow(),
            Status = JobStatus.Queued,
        };
        jobs.Add(record);
        RegisterNotify(type.Module);
        return new JobAccepted(record.Id, type.Module);
    }

    // 通知于事务提交后送达，确保执行器唤醒时作业记录已可见；同事务内重复通知由数据库自动合并
    private void RegisterNotify(string module) =>
        transaction.BeforeCommit(async (db, _, ct) =>
        {
            await db.Database.ExecuteSqlAsync($"SELECT pg_notify({MessagingChannels.Jobs}, {module})", ct);
            return null;
        });
}
