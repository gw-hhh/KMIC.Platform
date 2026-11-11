using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class TicketLine : Entity<TicketLineId>
{
    private TicketLine(TicketLineId id, string text)
        : base(id) =>
        Text = text;

    /// <summary>
    /// 明细内容
    /// </summary>
    public string Text { get; private set; }

    internal static TicketLine Create(string text) => new(TicketLineId.New(), text);

    internal void Change(string text) => Text = text;
}
