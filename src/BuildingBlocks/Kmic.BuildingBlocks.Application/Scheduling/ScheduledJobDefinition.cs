using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>
/// 校验过的定时任务
/// </summary>
/// <param name="Name">稳定名称</param>
/// <param name="Module">所属模块</param>
/// <param name="CommandType">发送的命令</param>
/// <param name="Cron">代码里的默认执行时间</param>
/// <param name="Create">按触发构造命令</param>
/// <param name="Misfire">错过触发时怎么办</param>
/// <param name="DisplayName">显示名，没写时用名称</param>
public sealed record ScheduledJobDefinition(
    string Name,
    string Module,
    Type CommandType,
    string Cron,
    Func<ScheduledRun, ICommand<ScheduledJobOutcome>> Create,
    Misfire Misfire,
    string DisplayName);
