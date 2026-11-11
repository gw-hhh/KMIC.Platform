using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class TicketStatus : Enumeration<TicketStatus>
{
    public static readonly TicketStatus Open = new("Open", "处理中");
    public static readonly TicketStatus Closed = new("Closed", "已关闭");

    private TicketStatus(string code, string name)
        : base(code, name)
    {
    }
}
