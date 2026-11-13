using Kmic.BuildingBlocks.Application.Scheduling;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.ScheduledJobs;

/// <summary>
/// 运维填的执行时间要满足的规则
/// </summary>
internal static class CronRules
{
    /// <summary>
    /// 两次执行之间至少隔这么久：秒级触发对这套系统没有业务意义，还可能压垮数据库
    /// </summary>
    public static readonly TimeSpan MinimumInterval = TimeSpan.FromMinutes(1);

    /// <summary>
    /// 预览时给出接下来几次触发
    /// </summary>
    public const int PreviewCount = 5;

    /// <summary>
    /// 检查预览结果：写法合法、以后还会触发、间隔不短于下限
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
            return OperationsErrors.CronInvalidWith(path, "这个时间以后不会再触发");
        }

        return preview.MinimumGap is { } gap && gap < MinimumInterval
            ? OperationsErrors.CronInvalidWith(path, "两次执行之间至少要隔 1 分钟")
            : Result.Success();
    }
}
