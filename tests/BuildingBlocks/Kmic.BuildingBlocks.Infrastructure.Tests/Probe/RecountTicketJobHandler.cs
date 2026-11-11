using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class RecountTicketJobHandler(ProbeHooks hooks) : IJobHandler<RecountTicketJob>
{
    public async Task<Result<JobOutcome>> ExecuteAsync(RecountTicketJob job, IJobContext context, CancellationToken ct)
    {
        for (var batch = context.GetCheckpoint<RecountCheckpoint>()?.NextBatch ?? 0; batch < job.Batches; batch++)
        {
            if (hooks.BeforeJobBatch is { } before)
            {
                await before(batch, ct);
            }

            ct.ThrowIfCancellationRequested();
            if (hooks.JobFailAtBatch == batch)
            {
                return TicketErrors.Rejected;
            }

            if (hooks.JobThrowAtBatch == batch)
            {
                throw new InvalidOperationException("模拟作业出错");
            }

            var sent = await context.SendBatchAsync(
                new AnnotateTicketCommand(TicketId.From(job.TicketId), $"第 {batch + 1} 批"), new RecountCheckpoint(batch + 1), batch + 1);
            if (sent.IsFailure)
            {
                return sent.Error;
            }
        }

        return new JobOutcome(Summary: new RecountSummary(job.Batches));
    }
}
