using Kmic.BuildingBlocks.Application.Jobs;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 不能续跑的作业：中断后标记失败
/// </summary>
/// <param name="TicketId">单据</param>
[BackgroundJob("BuildingBlocks.ExportTicket", Permission = ProbePermissions.Recount, Audited = true)]
internal sealed record ExportTicketJob(Guid TicketId) : IBackgroundJob;
