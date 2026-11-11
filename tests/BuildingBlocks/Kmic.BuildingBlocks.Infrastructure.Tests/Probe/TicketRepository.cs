using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class TicketRepository(ProbeDbContext context) : EfRepository<ProbeDbContext, Ticket, TicketId>(context)
{
    protected override Error NotFound => TicketErrors.NotFound;

    // 扣减额度：条件成立才扣，返回是否扣成
    public async Task<bool> TryDeductAsync(TicketId id, decimal amount, CancellationToken ct) =>
        await UpdateWhereAsync(t => t.Id == id && t.Amount >= amount, s => s.SetProperty(t => t.Amount, t => t.Amount - amount), ct) == 1;

    protected override IQueryable<Ticket> Aggregate(IQueryable<Ticket> set) => set.Include(t => t.Lines);
}
