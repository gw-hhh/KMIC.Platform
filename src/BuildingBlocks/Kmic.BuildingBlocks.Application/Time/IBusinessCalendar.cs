namespace Kmic.BuildingBlocks.Application.Time;

/// <summary>
/// 业务时区下的日期计算；领域层不知道时区，只接收这里换算好的日期或 UTC 区间
/// </summary>
public interface IBusinessCalendar
{
    /// <summary>
    /// 业务时区
    /// </summary>
    TimeZoneInfo Zone { get; }

    /// <summary>
    /// 业务时区下的今天
    /// </summary>
    DateOnly Today();

    /// <summary>
    /// 某一时刻在业务时区下属于哪一天
    /// </summary>
    DateOnly DateOf(DateTimeOffset instant);

    /// <summary>
    /// 一天的 UTC 区间：[当天开始, 次日开始)
    /// </summary>
    UtcRange DayRange(DateOnly date);

    /// <summary>
    /// 日期段（两端都含）对应的 UTC 半开区间
    /// </summary>
    UtcRange Range(DateOnly from, DateOnly toInclusive);

    /// <summary>
    /// 一个月的 UTC 区间
    /// </summary>
    UtcRange MonthRange(int year, int month);

    /// <summary>
    /// 这一天开始的 UTC 时刻
    /// </summary>
    DateTimeOffset StartOf(DateOnly date);
}
