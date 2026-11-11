using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

[SystemOnly]
[NotAudited("测试用的定时任务命令")]
public sealed record PurgeThingsCommand(DateTimeOffset ScheduledFireTime) : ICommand<ScheduledJobOutcome>;
