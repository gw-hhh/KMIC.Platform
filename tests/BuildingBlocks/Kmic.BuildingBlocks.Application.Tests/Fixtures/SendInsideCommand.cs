using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

/// <summary>
/// 在处理器里再发命令，用来验证运行时的拦截
/// </summary>
[AllowAuthenticated]
[AllowSystem]
[NotAudited("测试")]
public sealed record SendInsideCommand(bool SendCommand) : ICommand;
