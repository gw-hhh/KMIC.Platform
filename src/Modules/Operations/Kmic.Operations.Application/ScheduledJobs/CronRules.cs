using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>
/// 定时任务的 Cron 校验规则
/// </summary>
internal static class CronRules
{
    /// <summary>
    /// 最小执行间隔，限制频繁触发造成的数据库负载
    /// </summary>
    public static readonly TimeSpan MinimumInterval = TimeSpan.FromMinutes(1);

    /// <summary>
    /// 预览触发次数上限
    /// </summary>
    public const int PreviewCount = 5;

    /// <summary>
    /// 校验 Cron 格式、未来触发时间和最小执行间隔
    /// </summary>
    /// <param name="preview">对表达式的预览</param>
    /// <param name="path">出错时指向的字段</param>
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
            ? OperationsErrors.CronInvalidWith(path, "执行间隔不能少于 1 分钟")
            : Result.Success();
    }
}
