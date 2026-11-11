using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 改标题并经 IQuerySender 读取标题；ReadFirst 决定先读还是先改
/// </summary>
[AllowAuthenticated]
[NotAudited("测试命令")]
internal sealed record RetitleAndReadCommand(TicketId Id, string Title, bool ReadFirst) : ICommand<string>;
