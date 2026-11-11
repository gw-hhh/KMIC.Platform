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

/// <summary>
/// 提交后台作业：作业行加进本模块的写入上下文，随命令事务提交
/// </summary>
internal sealed class BackgroundJobs(
    BackgroundJobCatalog catalog,
    IExecutionContext execution,
    ModuleWriteContexts contexts,
    CommandTransaction transaction,
    TimeProvider time) : IBackgroundJobs
{
    public async Task<Result<Guid>> EnqueueAsync(IBackgroundJob job, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (!execution.InCommand)
        {
            throw new InvalidOperationException("后台作业只能在命令里提交，要和业务数据一起提交。");
        }

        var type = catalog.Describe(job.GetType());

        // 只有命令所在模块的上下文会被保存，别的模块的作业行加进去也不会落库
        if (!string.Equals(transaction.Module, type.Module, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"作业 {type.Name} 属于 {type.Module} 模块，只能由该模块的命令提交。");
        }

        var user = execution.User;
        if (type.Attribute.SystemOnly && !user.IsSystem)
        {
            throw new InvalidOperationException($"作业 {type.Name} 只能由系统身份提交。");
        }

        // 提交命令自己也会声明权限点，这里再按作业的权限点查一次，执行时同样要有
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
        return record.Id;
    }

    // 通知在提交时才送达，执行器被唤醒时一定能读到作业行。同一事务里重复的通知数据库会合并
    private void RegisterNotify(string module) =>
        transaction.BeforeCommit(async (db, _, ct) =>
        {
            await db.Database.ExecuteSqlAsync($"SELECT pg_notify({MessagingChannels.Jobs}, {module})", ct);
            return null;
        });
}
