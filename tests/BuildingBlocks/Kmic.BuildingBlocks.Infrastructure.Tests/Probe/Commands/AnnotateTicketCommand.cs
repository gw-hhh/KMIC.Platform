using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 给单据加一行备注，作业的每一批发送它
/// </summary>
/// <param name="Id">单据</param>
/// <param name="Text">备注</param>
[RequirePermission(ProbePermissions.Recount)]
[NotAudited("测试命令")]
internal sealed record AnnotateTicketCommand(TicketId Id, string Text) : ICommand;
