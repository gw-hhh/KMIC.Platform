using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

/// <summary>
/// 处理器里又发命令：用来验证 05-02 D5 的运行时守卫
/// </summary>
[AllowAuthenticated]
[AllowSystem]
[NotAudited("测试")]
public sealed record SendInsideCommand(bool SendCommand) : ICommand;
