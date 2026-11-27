using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>已校验的定时任务定义；<paramref name="Cron"/> 为代码配置的默认表达式，<paramref name="Create"/> 根据触发信息构造命令。</summary>
public sealed record ScheduledJobDefinition(
    string Name,
    string Module,
    Type CommandType,
    string Cron,
    Func<ScheduledRun, ICommand<ScheduledJobOutcome>> Create,
    Misfire Misfire,
    string DisplayName);
