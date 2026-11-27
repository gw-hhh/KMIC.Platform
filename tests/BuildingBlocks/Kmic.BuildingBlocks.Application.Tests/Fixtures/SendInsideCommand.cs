using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

// 在处理器内再发命令，用于验证运行时拦截
[AllowAuthenticated]
[AllowSystem]
[NotAudited("测试")]
public sealed record SendInsideCommand(bool SendCommand) : ICommand;
