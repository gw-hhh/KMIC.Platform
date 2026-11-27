using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;
using Kmic.Operations.Domain.Jobs;

namespace Kmic.Operations.Application.ScheduledJobs.Set;

public sealed class SetJobOverrideCommandHandler(
    IScheduledJobControl control,
    IScheduledJobSettingRepository settings,
    TimeProvider time,
    IAfterCommit afterCommit) : ICommandHandler<SetJobOverrideCommand, VersionedResponse<ScheduledJobSettingId>>
{
    public async ValueTask<Result<VersionedResponse<ScheduledJobSettingId>>> Handle(SetJobOverrideCommand command, CancellationToken cancellationToken)
    {
        // 已从代码中移除的任务仅保留配置记录，修改后无对应触发器需要同步
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
