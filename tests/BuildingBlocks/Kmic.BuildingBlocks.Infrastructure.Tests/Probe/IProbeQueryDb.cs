namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal interface IProbeQueryDb
{
    IQueryable<Ticket> Tickets { get; }
}
