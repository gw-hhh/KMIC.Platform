namespace Kmic.BuildingBlocks.Application.Time;

public sealed class BusinessCalendar(TimeProvider time, TimeZoneInfo zone) : IBusinessCalendar
{
    public TimeZoneInfo Zone { get; } = zone ?? throw new ArgumentNullException(nameof(zone));

    public DateOnly Today() => DateOf(time.GetUtcNow());

    public DateOnly DateOf(DateTimeOffset instant) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, Zone).DateTime);

    public UtcRange DayRange(DateOnly date) => Range(date, date);

    public UtcRange Range(DateOnly from, DateOnly toInclusive)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(from, toInclusive);
        return new UtcRange(StartOf(from), StartOf(toInclusive.AddDays(1)));
    }

    public UtcRange MonthRange(int year, int month)
    {
        var first = new DateOnly(year, month, 1);
        return new UtcRange(StartOf(first), StartOf(first.AddMonths(1)));
    }

    public DateTimeOffset StartOf(DateOnly date)
    {
        var midnight = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(midnight, OffsetAtMidnight(midnight)).ToUniversalTime();
    }

    private TimeSpan OffsetAtMidnight(DateTime midnight)
    {
        // 夏令时结束当日零点会出现两次，取较早一次作为当日起点
        if (Zone.IsAmbiguousTime(midnight))
        {
            return Zone.GetAmbiguousTimeOffsets(midnight).Max();
        }

        // 夏令时开始当日零点被跳过，按跳变前偏移量换算，恰好对应跳变时刻
        return Zone.IsInvalidTime(midnight) ? Zone.GetUtcOffset(midnight.AddDays(-1)) : Zone.GetUtcOffset(midnight);
    }
}
