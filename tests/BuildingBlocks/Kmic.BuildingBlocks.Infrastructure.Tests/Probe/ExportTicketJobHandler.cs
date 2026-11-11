using Kmic.BuildingBlocks.Application.Exports;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 把提交人能看到的单据标题逐行导出到钩子里
/// </summary>
internal sealed class ExportTicketJobHandler(ProbeHooks hooks, IExportReader<IProbeQueryDb> reader) : IJobHandler<ExportTicketJob>
{
    public async Task<Result<JobOutcome>> ExecuteAsync(ExportTicketJob job, IJobContext context, CancellationToken ct)
    {
        if (hooks.BeforeJobBatch is { } before)
        {
            await before(0, ct);
        }

        var exported = await reader.ReadAsync(
            db => db.Tickets.OrderBy(t => t.Number).Select(t => t.Title),
            (title, _) =>
            {
                hooks.Exported.Enqueue(title);
                return ValueTask.CompletedTask;
            },
            ct);
        if (exported.IsFailure)
        {
            return exported.Error;
        }

        await context.ReportProgressAsync(exported.Value, exported.Value, "已导出");
        return new JobOutcome(Summary: new ExportSummary(exported.Value));
    }
}
