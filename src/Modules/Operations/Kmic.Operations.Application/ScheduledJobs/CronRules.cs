using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.ScheduledJobs;

internal static class CronRules
{
    // 最小触发间隔，避免高频调度加重数据库负载
    public static readonly TimeSpan MinimumInterval = TimeSpan.FromMinutes(1);

    public const int PreviewCount = 5;

    public static Result Check(CronPreview preview, string path)
    {
        ArgumentNullException.ThrowIfNull(preview);

        if (!preview.IsValid)
        {
            return OperationsErrors.CronInvalidWith(path, $"不是合法的 Cron 表达式：{preview.Problem}");
        }

        if (preview.NextFireTimes.Count == 0)
        {
            return OperationsErrors.CronInvalidWith(path, "该表达式未来不会再触发");
        }

        return preview.MinimumGap is { } gap && gap < MinimumInterval
            ? OperationsErrors.CronInvalidWith(path, "触发间隔不能少于 1 分钟")
            : Result.Success();
    }
}
