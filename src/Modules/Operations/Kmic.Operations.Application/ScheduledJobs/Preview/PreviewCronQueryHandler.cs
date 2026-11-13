using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.ScheduledJobs.Preview;

/// <summary>
/// 处理 <see cref="PreviewCronQuery"/>
/// </summary>
public sealed class PreviewCronQueryHandler(IScheduledJobControl control, TimeProvider time) : IQueryHandler<PreviewCronQuery, CronPreviewResponse>
{
    /// <inheritdoc />
    public ValueTask<Result<CronPreviewResponse>> Handle(PreviewCronQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var preview = control.Preview(query.Cron.Trim(), time.GetUtcNow(), CronRules.PreviewCount);
        var verdict = CronRules.Check(preview, "cron");
        Result<CronPreviewResponse> result = verdict.IsFailure ? verdict.Error : new CronPreviewResponse(preview.NextFireTimes);
        return ValueTask.FromResult(result);
    }
}
