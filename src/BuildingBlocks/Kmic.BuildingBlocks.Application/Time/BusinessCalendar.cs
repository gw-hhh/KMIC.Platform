namespace Kmic.BuildingBlocks.Application.Time;

/// <summary>
/// 业务日历
/// </summary>
/// <param name="time">时间来源</param>
/// <param name="zone">业务时区</param>
public sealed class BusinessCalendar(TimeProvider time, TimeZoneInfo zone) : IBusinessCalendar
{
    /// <inheritdoc />
    public TimeZoneInfo Zone { get; } = zone ?? throw new ArgumentNullException(nameof(zone));

    /// <inheritdoc />
    public DateOnly Today() => DateOf(time.GetUtcNow());

    /// <inheritdoc />
    public DateOnly DateOf(DateTimeOffset instant) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, Zone).DateTime);

    /// <inheritdoc />
    public UtcRange DayRange(DateOnly date) => Range(date, date);

    /// <inheritdoc />
    public UtcRange Range(DateOnly from, DateOnly toInclusive)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(from, toInclusive);
        return new UtcRange(StartOf(from), StartOf(toInclusive.AddDays(1)));
    }

    /// <inheritdoc />
    public UtcRange MonthRange(int year, int month)
    {
        var first = new DateOnly(year, month, 1);
        return new UtcRange(StartOf(first), StartOf(first.AddMonths(1)));
    }

    /// <inheritdoc />
    public DateTimeOffset StartOf(DateOnly date)
    {
        var midnight = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(midnight, OffsetAtMidnight(midnight)).ToUniversalTime();
    }

    private TimeSpan OffsetAtMidnight(DateTime midnight)
    {
        // 夏令时结束那天零点出现两次，从较早的那次算起
        if (Zone.IsAmbiguousTime(midnight))
        {
            return Zone.GetAmbiguousTimeOffsets(midnight).Max();
        }

        // 夏令时开始那天零点被跳过，按跳变前的偏移换算，正好落在跳变那一刻
        return Zone.IsInvalidTime(midnight) ? Zone.GetUtcOffset(midnight.AddDays(-1)) : Zone.GetUtcOffset(midnight);
    }
}
