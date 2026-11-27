using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.ScheduledJobs.Preview;

public sealed class PreviewCronQueryHandler(IScheduledJobControl control, TimeProvider time) : IQueryHandler<PreviewCronQuery, CronPreviewResponse>
{
    public ValueTask<Result<CronPreviewResponse>> Handle(PreviewCronQuery query, CancellationToken cancellationToken)
    {
        var preview = control.Preview(query.Cron.Trim(), time.GetUtcNow(), CronRules.PreviewCount);
        var verdict = CronRules.Check(preview, "cron");
        Result<CronPreviewResponse> result = verdict.IsFailure ? verdict.Error : new CronPreviewResponse(preview.NextFireTimes);
        return ValueTask.FromResult(result);
    }
}
