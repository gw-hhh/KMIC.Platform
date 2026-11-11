using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 在命令里调用外部系统，用来验证守卫
/// </summary>
[AllowAuthenticated]
[NotAudited("测试命令")]
internal sealed record CallPartnerCommand : ICommand;
