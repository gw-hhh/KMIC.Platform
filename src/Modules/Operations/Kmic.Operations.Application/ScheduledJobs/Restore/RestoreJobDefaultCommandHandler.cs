using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.ScheduledJobs.Restore;

/// <summary>
/// 恢复定时任务默认设置
/// </summary>
public sealed class RestoreJobDefaultCommandHandler(
    IScheduledJobControl control,
    IScheduledJobSettingRepository settings,
    IAfterCommit afterCommit) : ICommandHandler<RestoreJobDefaultCommand, VersionedResponse<ScheduledJobSettingId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<ScheduledJobSettingId>>> Handle(RestoreJobDefaultCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!control.IsDeclared(command.JobName))
        {
            return OperationsErrors.JobNotFound;
        }

        var loaded = await settings.GetByJobNameForUpdateAsync(command.JobName, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        // 没有自定义设置记录时，按任务不存在处理
        if (loaded.Value is not { } setting)
        {
            return OperationsErrors.JobNotFound;
        }

        setting.RestoreDefault();
        afterCommit.Enqueue(ct => new ValueTask(control.SyncAsync(ct)));
        return new VersionedResponse<ScheduledJobSettingId>(setting.Id);
    }
}
