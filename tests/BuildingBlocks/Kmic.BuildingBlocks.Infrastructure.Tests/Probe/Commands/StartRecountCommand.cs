using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 提交重新统计的作业
/// </summary>
/// <param name="Id">单据</param>
/// <param name="Batches">批数</param>
/// <param name="Fail">提交之后返回失败，作业应该随命令一起回滚</param>
/// <param name="Export">改为提交不能续跑的导出作业</param>
[RequirePermission(ProbePermissions.Recount)]
[NotAudited("测试命令")]
internal sealed record StartRecountCommand(TicketId Id, int Batches = 1, bool Fail = false, bool Export = false) : ICommand<Guid>;
