using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

[SystemOnly]
[NotAudited("测试用的系统命令")]
public sealed record SyncThingCommand : ICommand;
