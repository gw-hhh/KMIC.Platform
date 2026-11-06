using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Spike06.Tests;

// 原型 #6：KmicFunctions.ToBusinessDate 翻译为 AT TIME ZONE，按业务日期分组与 .NET 的时区换算一致
public sealed class BusinessDateTests(ClassDatabase db) : IClassFixture<ClassDatabase>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("Asia/Shanghai", new[] { "2026-11-04T16:00:00Z", "2026-11-05T15:59:59Z", "2026-11-05T16:00:00Z", "2026-11-06T03:00:00Z" })]
    // 美国东部 2026-11-01 02:00 结束夏令时：同一天有 25 小时
    [InlineData("America/New_York", new[] { "2026-11-01T03:59:59Z", "2026-11-01T04:00:00Z", "2026-11-02T04:59:59Z", "2026-11-02T05:00:00Z" })]
    public async Task Group_by_business_date_matches_dotnet(string timeZone, string[] instants)
    {
        var events = instants.Select(i => new Event { Id = Guid.CreateVersion7(), OccurredAt = DateTimeOffset.Parse(i, System.Globalization.CultureInfo.InvariantCulture) }).ToList();
        await using (var setup = new EventContext(db.ConnectionString, timeZone))
        {
            setup.Events.AddRange(events);
            await setup.SaveChangesAsync(Ct);
        }

        await using var context = new EventContext(db.ConnectionString, timeZone);
        var ids = events.Select(e => e.Id).ToList();
        var query = context.Events
            .Where(e => ids.Contains(e.Id))
            .GroupBy(e => KmicFunctions.ToBusinessDate(e.OccurredAt))
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .OrderBy(x => x.Day);
        var sql = query.ToQueryString();
        TestContext.Current.TestOutputHelper?.WriteLine(sql);

        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        var expected = events
            .GroupBy(e => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(e.OccurredAt, zone).DateTime))
            .Select(g => (g.Key, g.Count()))
            .OrderBy(x => x.Key)
            .ToList();
        var actual = (await query.ToListAsync(Ct)).Select(x => (x.Day, x.Count)).ToList();

        sql.ShouldContain($"AT TIME ZONE '{timeZone}'");
        actual.ShouldBe(expected);
    }
}
