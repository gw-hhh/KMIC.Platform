using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Echo.Probe;

// 收到单据快照后发布 TicketMirrored，测试通过统计其出现次数验证执行次数
[SystemOnly]
[NotAudited("测试命令")]
public sealed record MirrorTicketCommand(Guid TicketId, string Title, long SourceVersion) : ICommand;
