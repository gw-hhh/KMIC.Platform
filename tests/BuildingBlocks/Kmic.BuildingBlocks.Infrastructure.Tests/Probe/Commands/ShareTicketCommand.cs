using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 依次改成每个标题，每改一次发布一次快照
/// </summary>
/// <param name="Id">单据</param>
/// <param name="Titles">依次改成的标题</param>
/// <param name="Fail">发布之后返回失败</param>
[AllowAuthenticated]
[NotAudited("测试命令")]
internal sealed record ShareTicketCommand(TicketId Id, IReadOnlyList<string> Titles, bool Fail = false) : ICommand;
