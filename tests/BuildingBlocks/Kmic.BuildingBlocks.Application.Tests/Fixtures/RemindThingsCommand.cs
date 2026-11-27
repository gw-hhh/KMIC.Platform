using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

// 允许系统执行但非系统专用，不能用作定时任务
[RequirePermission("BuildingBlocks.Thing.Remind")]
[AllowSystem]
[NotAudited("测试")]
public sealed record RemindThingsCommand : ICommand<ScheduledJobOutcome>;
