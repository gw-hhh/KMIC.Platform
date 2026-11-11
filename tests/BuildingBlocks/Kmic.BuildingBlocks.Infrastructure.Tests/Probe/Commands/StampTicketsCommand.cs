using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 定时任务发送的命令：记下触发时刻，给指定的单据加一行
/// </summary>
/// <param name="ScheduledFireTime">计划触发时刻</param>
[SystemOnly]
[NotAudited("测试命令")]
internal sealed record StampTicketsCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;
