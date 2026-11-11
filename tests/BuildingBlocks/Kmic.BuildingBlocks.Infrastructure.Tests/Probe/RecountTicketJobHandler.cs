using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

internal sealed class RecountTicketJobHandler : IJobHandler<RecountTicketJob>
{
    public Task<Result<JobOutcome>> ExecuteAsync(RecountTicketJob job, IJobContext context, CancellationToken ct) =>
        Task.FromResult<Result<JobOutcome>>(new JobOutcome());
}
