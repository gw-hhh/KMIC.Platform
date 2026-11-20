using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 改标题并发布一次快照，声明了消息写入被软线熔断时仍可执行
/// </summary>
/// <param name="Id">单据</param>
/// <param name="Title">新标题</param>
[AllowAuthenticated]
[NotAudited("测试命令")]
[ExemptFromMessagingSuspension]
internal sealed record ShareTicketExemptCommand(TicketId Id, string Title) : ICommand;
