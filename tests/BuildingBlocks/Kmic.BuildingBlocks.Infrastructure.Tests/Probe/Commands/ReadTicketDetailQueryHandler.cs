using System.Globalization;
using Kmic.BuildingBlocks.Application.Auditing;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

internal sealed class ReadTicketDetailQueryHandler(IProbeQueryDb db, IAuditContext audit) : IQueryHandler<ReadTicketDetailQuery, string>
{
    public async ValueTask<Result<string>> Handle(ReadTicketDetailQuery query, CancellationToken cancellationToken)
    {
        var title = await db.Tickets.Where(t => t.Id == query.Id).Select(t => t.Title).SingleOrDefaultAsync(cancellationToken);
        if (title is null)
        {
            return TicketErrors.NotFound;
        }

        audit.AddTarget("Ticket", query.Id.Value);
        audit.Add("filter", "只看标题");
        if (query.TooManyDetails)
        {
            for (var i = 0; i < 400; i++)
            {
                audit.Add("detail" + i.ToString(CultureInfo.InvariantCulture), new string('x', 1000));
            }
        }

        return title;
    }
}
