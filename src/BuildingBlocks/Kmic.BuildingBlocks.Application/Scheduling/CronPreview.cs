namespace Kmic.BuildingBlocks.Application.Scheduling;

/// <summary>Cron 表达式校验与触发时间预览结果。</summary>
/// <param name="Problem">表达式错误说明，合法时为 null。</param>
/// <param name="NextFireTimes">后续触发时刻列表，合法但不再触发时为空。</param>
public sealed record CronPreview(string? Problem, IReadOnlyList<DateTimeOffset> NextFireTimes)
{
    public bool IsValid => Problem is null;

    /// <summary>相邻两次触发的最短间隔，触发次数不足两次时为 null。</summary>
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
