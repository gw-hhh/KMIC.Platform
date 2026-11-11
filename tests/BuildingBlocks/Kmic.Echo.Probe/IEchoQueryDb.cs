namespace Kmic.Echo.Probe;

internal interface IEchoQueryDb
{
    IQueryable<TicketSummary> Tickets { get; }
}
