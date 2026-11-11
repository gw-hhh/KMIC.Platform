using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class GetTicketTitleQueryHandler(IProbeQueryDb db) : IQueryHandler<GetTicketTitleQuery, string>
{
    public async ValueTask<Result<string>> Handle(GetTicketTitleQuery query, CancellationToken cancellationToken)
    {
        var title = await db.Tickets.Where(t => t.Id == query.Id).Select(t => t.Title).SingleOrDefaultAsync(cancellationToken);
        return title is null ? TicketErrors.NotFound : title;
    }
}
