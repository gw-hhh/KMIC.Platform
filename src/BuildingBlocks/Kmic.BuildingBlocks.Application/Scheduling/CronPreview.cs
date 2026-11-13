namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>
/// 检查 Cron 表达式的结果
/// </summary>
/// <param name="Problem">写得不对时的说明；合法时为空</param>
/// <param name="NextFireTimes">接下来的触发时刻，按时间顺序；表达式合法但以后不会再触发时为空</param>
public sealed record CronPreview(string? Problem, IReadOnlyList<DateTimeOffset> NextFireTimes)
{
    /// <summary>
    /// 写法是否合法
    /// </summary>
    public bool IsValid => Problem is null;

    /// <summary>
    /// 相邻两次触发之间最短的间隔；少于两次触发时为空
    /// </summary>
    /// <remarks>
    /// 用预览出的触发时刻算。同一分钟里的秒位取值都一样，所以一分钟内触发不止一次的表达式，
    /// 头几次触发里就有这样的间隔，不用往后看很多次
    /// </remarks>
    public TimeSpan? MinimumGap
    {
        get
        {
            if (NextFireTimes.Count < 2)
            {
                return null;
            }

            var gap = TimeSpan.MaxValue;
            for (var i = 1; i < NextFireTimes.Count; i++)
            {
                var between = NextFireTimes[i] - NextFireTimes[i - 1];
                if (between < gap)
                {
                    gap = between;
                }
            }

            return gap;
        }
    }
}
