using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Echo.Probe;

/// <summary>
/// 收到单据快照后，Echo 发布一条 TicketMirrored，测试数它出现的次数就知道处理执行了几次
/// </summary>
[SystemOnly]
[NotAudited("测试命令")]
public sealed record MirrorTicketCommand(Guid TicketId, string Title, long SourceVersion) : ICommand;
