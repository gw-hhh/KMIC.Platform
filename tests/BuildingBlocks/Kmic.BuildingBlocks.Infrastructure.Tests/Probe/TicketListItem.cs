namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 单据列表项，分页测试用
/// </summary>
internal sealed record TicketListItem(TicketId Id, string Number, string Title, decimal Amount);
