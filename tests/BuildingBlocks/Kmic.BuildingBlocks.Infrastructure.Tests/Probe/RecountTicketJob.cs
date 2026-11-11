using Kmic.BuildingBlocks.Application.Jobs;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 分批给单据加备注行，每批一个命令，检查点记录做到第几批
/// </summary>
/// <param name="TicketId">单据</param>
/// <param name="Batches">批数</param>
[BackgroundJob("BuildingBlocks.RecountTicket", Permission = ProbePermissions.Recount, Resumable = true, MaxConcurrentPerUser = 2)]
internal sealed record RecountTicketJob(Guid TicketId, int Batches) : IBackgroundJob;
