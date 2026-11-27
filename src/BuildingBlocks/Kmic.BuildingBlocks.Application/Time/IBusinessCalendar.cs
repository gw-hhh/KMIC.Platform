namespace Kmic.BuildingBlocks.Application.Time;

/// <summary>业务日历；领域层不直接处理时区，仅接收此处换算后的业务日期或 UTC 区间。</summary>
public interface IBusinessCalendar
{
    TimeZoneInfo Zone { get; }

    /// <summary>获取当前业务日期。</summary>
    DateOnly Today();

    /// <summary>获取指定时刻对应的业务日期。</summary>
    DateOnly DateOf(DateTimeOffset instant);

    /// <summary>获取指定业务日期的 UTC 时间区间。</summary>
    UtcRange DayRange(DateOnly date);

    /// <summary>获取指定日期范围的 UTC 时间区间（含首尾日期）。</summary>
    UtcRange Range(DateOnly from, DateOnly toInclusive);

    /// <summary>获取指定月份的 UTC 时间区间。</summary>
    UtcRange MonthRange(int year, int month);

    /// <summary>获取指定业务日期零点对应的 UTC 时刻。</summary>
    DateTimeOffset StartOf(DateOnly date);
}
