using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 关闭单据，会发布领域事件并登记一个提交后的动作
/// </summary>
[AllowAuthenticated]
[NotAudited("测试命令")]
internal sealed record CloseTicketCommand(TicketId Id) : ICommand;
