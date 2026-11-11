using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class ExportTicketJobHandler(ProbeHooks hooks) : IJobHandler<ExportTicketJob>
{
    public async Task<Result<JobOutcome>> ExecuteAsync(ExportTicketJob job, IJobContext context, CancellationToken ct)
    {
        if (hooks.BeforeJobBatch is { } before)
        {
            await before(0, ct);
        }

        await context.ReportProgressAsync(1, 1, "已导出");
        return new JobOutcome();
    }
}
