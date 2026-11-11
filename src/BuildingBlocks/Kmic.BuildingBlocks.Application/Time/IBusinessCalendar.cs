namespace Kmic.BuildingBlocks.Application.Time;

/// <summary>
/// 业务日历
/// </summary>
/// <remarks>
/// 领域层不处理时区，只接收这里换算好的日期或 UTC 区间
/// </remarks>
public interface IBusinessCalendar
{
    /// <summary>
    /// 业务时区
    /// </summary>
    TimeZoneInfo Zone { get; }

    /// <summary>
    /// 今天
    /// </summary>
    DateOnly Today();

    /// <summary>
    /// 某个时刻是哪一天
    /// </summary>
    DateOnly DateOf(DateTimeOffset instant);

    /// <summary>
    /// 某一天的 UTC 区间
    /// </summary>
    UtcRange DayRange(DateOnly date);

    /// <summary>
    /// 日期段的 UTC 区间，两端日期都含
    /// </summary>
    UtcRange Range(DateOnly from, DateOnly toInclusive);

    /// <summary>
    /// 某个月的 UTC 区间
    /// </summary>
    UtcRange MonthRange(int year, int month);

    /// <summary>
    /// 某一天开始的 UTC 时刻
    /// </summary>
    DateTimeOffset StartOf(DateOnly date);
}
