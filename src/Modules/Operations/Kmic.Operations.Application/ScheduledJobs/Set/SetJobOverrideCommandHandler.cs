using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.ScheduledJobs.Set;

/// <summary>
/// 处理 <see cref="SetJobOverrideCommand"/>
/// </summary>
public sealed class SetJobOverrideCommandHandler(
    IScheduledJobControl control,
    IScheduledJobSettingRepository settings,
    TimeProvider time,
    IAfterCommit afterCommit) : ICommandHandler<SetJobOverrideCommand, VersionedResponse<ScheduledJobSettingId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<ScheduledJobSettingId>>> Handle(SetJobOverrideCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // 已经从代码里删除的任务只剩设置行，改了也没有触发器可同步
        if (!control.IsDeclared(command.JobName))
        {
            return OperationsErrors.JobNotFound;
        }

        var cron = command.Cron?.Trim();
        if (cron is not null)
        {
            var verdict = CronRules.Check(control.Preview(cron, time.GetUtcNow(), CronRules.PreviewCount), "cron");
            if (verdict.IsFailure)
            {
                return verdict.Error;
            }
        }

        var loaded = await settings.GetByJobNameForUpdateAsync(command.JobName, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var setting = loaded.Value;
        if (setting is null)
        {
            setting = ScheduledJobSetting.Create(command.JobName, command.Enabled, cron);
            settings.Add(setting);
        }
        else
        {
            setting.Apply(command.Enabled, cron);
        }

        afterCommit.Enqueue(ct => new ValueTask(control.SyncAsync(ct)));
        return new VersionedResponse<ScheduledJobSettingId>(setting.Id);
    }
}
