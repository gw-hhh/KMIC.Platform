using Kmic.BuildingBlocks.Application.Time;
using Microsoft.Extensions.Time.Testing;
using Shouldly;

namespace Kmic.BuildingBlocks.Application.Tests.Time;

public sealed class BusinessCalendarTests
{
    private static readonly TimeZoneInfo _plus8 = TimeZoneInfo.CreateCustomTimeZone("Test/Plus8", TimeSpan.FromHours(8), "UTC+8", "UTC+8");

    // 9 月 6 日零点跳到 1 点，4 月 5 日 1 点退回零点：两个边界都落在零点
    private static readonly TimeZoneInfo _midnightDst = TimeZoneInfo.CreateCustomTimeZone(
        "Test/MidnightDst",
        TimeSpan.FromHours(-4),
        "Test",
        "Test Standard",
        "Test Daylight",
        [
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                new DateTime(2020, 1, 1),
                new DateTime(2030, 12, 31),
                TimeSpan.FromHours(1),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 0, 0, 0), 9, 6),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(1, 1, 1, 1, 0, 0), 4, 5)),
        ]);

    [Fact]
    public void Today_FollowsBusinessZoneAcrossUtcMidnight()
    {
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 11, 10, 15, 59, 59, TimeSpan.Zero));
        var calendar = new BusinessCalendar(time, _plus8);

        calendar.Today().ShouldBe(new DateOnly(2026, 11, 10));
        time.Advance(TimeSpan.FromSeconds(1));
        calendar.Today().ShouldBe(new DateOnly(2026, 11, 11));
    }

    [Fact]
    public void Ranges_AreHalfOpenInUtc()
    {
        var calendar = new BusinessCalendar(TimeProvider.System, _plus8);

        calendar.DayRange(new DateOnly(2026, 11, 10)).ShouldBe(new UtcRange(Utc(2026, 11, 9, 16), Utc(2026, 11, 10, 16)));
        calendar.Range(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30)).ShouldBe(new UtcRange(Utc(2026, 10, 31, 16), Utc(2026, 11, 30, 16)));
        calendar.MonthRange(2026, 12).ShouldBe(new UtcRange(Utc(2026, 11, 30, 16), Utc(2026, 12, 31, 16)));

        var day = calendar.DayRange(new DateOnly(2026, 11, 10));
        day.Contains(day.Start).ShouldBeTrue();
        day.Contains(day.EndExclusive).ShouldBeFalse();
    }

    [Fact]
    public void Range_EndBeforeStart_Throws() =>
        Should.Throw<ArgumentOutOfRangeException>(
            () => new BusinessCalendar(TimeProvider.System, _plus8).Range(new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 1)));

    [Fact]
    public void StartOf_SkippedMidnight_IsTheMomentOfTheJump()
    {
        var calendar = new BusinessCalendar(TimeProvider.System, _midnightDst);

        var start = calendar.StartOf(new DateOnly(2026, 9, 6));

        start.ShouldBe(Utc(2026, 9, 6, 4));
        calendar.DateOf(start).ShouldBe(new DateOnly(2026, 9, 6));
        calendar.DateOf(start.AddTicks(-1)).ShouldBe(new DateOnly(2026, 9, 5));
    }

    [Fact]
    public void StartOf_RepeatedMidnight_IsTheEarlierOne()
    {
        var calendar = new BusinessCalendar(TimeProvider.System, _midnightDst);

        var start = calendar.StartOf(new DateOnly(2026, 4, 5));

        start.ShouldBe(Utc(2026, 4, 5, 3));
        calendar.DateOf(start).ShouldBe(new DateOnly(2026, 4, 5));
        calendar.DateOf(start.AddTicks(-1)).ShouldBe(new DateOnly(2026, 4, 4));
    }

    private static DateTimeOffset Utc(int year, int month, int day, int hour) => new(year, month, day, hour, 0, 0, TimeSpan.Zero);
}
