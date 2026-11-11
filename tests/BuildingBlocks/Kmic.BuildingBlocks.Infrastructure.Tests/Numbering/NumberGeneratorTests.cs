using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Numbering;

/// <summary>
/// 每个测试用自己的规则周期（测试时钟指到不同的年份），同一个库里的计数互不影响。业务时区是上海（UTC+8）
/// </summary>
public sealed class NumberGeneratorTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _user = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task SamePeriod_CountsUp_NewMonthStartsOver()
    {
        At(2031, 3, 10, 2);

        (await TakeAsync(ProbeNumbers.Ticket)).ShouldBe("TK-203103-001");
        (await TakeAsync(ProbeNumbers.Ticket)).ShouldBe("TK-203103-002");

        At(2031, 4, 1, 2);
        (await TakeAsync(ProbeNumbers.Ticket)).ShouldBe("TK-203104-001");
        (await TakeAsync(ProbeNumbers.Ticket)).ShouldBe("TK-203104-002");
    }

    [Fact]
    public async Task BusinessTimeZoneMidnight_DecidesThePeriod()
    {
        // UTC 15:59 在上海是 23:59，还在当天；UTC 16:00 已经是第二天
        _harness.Time.SetUtcNow(new DateTimeOffset(2032, 5, 31, 15, 59, 0, TimeSpan.Zero));
        (await TakeAsync(ProbeNumbers.Daily)).ShouldBe("DY320531-01");

        _harness.Time.SetUtcNow(new DateTimeOffset(2032, 5, 31, 16, 0, 0, TimeSpan.Zero));
        (await TakeAsync(ProbeNumbers.Daily)).ShouldBe("DY320601-01");
    }

    [Fact]
    public async Task Partitions_CountIndependently()
    {
        At(2033, 6, 1, 2);

        (await TakeAsync(ProbeNumbers.Review, "A")).ShouldBe("RV-33-A-01");
        (await TakeAsync(ProbeNumbers.Review, "B")).ShouldBe("RV-33-B-01");
        (await TakeAsync(ProbeNumbers.Review, "A")).ShouldBe("RV-33-A-02");
    }

    [Fact]
    public async Task SequencePastItsDigits_NumberGrowsLonger()
    {
        At(2034, 6, 1, 2);
        (await TakeAsync(ProbeNumbers.Review, "C")).ShouldBe("RV-34-C-01");
        await SetLastValueAsync(ProbeNumbers.Review, "2034", "C", 98);

        (await TakeAsync(ProbeNumbers.Review, "C")).ShouldBe("RV-34-C-99");
        (await TakeAsync(ProbeNumbers.Review, "C")).ShouldBe("RV-34-C-100");
    }

    [Fact]
    public async Task CommandFails_NumberNotConsumed()
    {
        At(2035, 6, 1, 2);
        (await TakeAsync(ProbeNumbers.Ticket)).ShouldBe("TK-203506-001");

        await using (var work = _harness.Begin(_user))
        {
            (await work.Sender.SendAsync(new TakeNumberCommand(ProbeNumbers.Ticket, FailAfterTaking: true), Ct)).ShouldBeFailure(TicketErrors.Rejected);
        }

        (await TakeAsync(ProbeNumbers.Ticket)).ShouldBe("TK-203506-002");
    }

    [Fact]
    public async Task Concurrent_NoDuplicatesNoGaps()
    {
        At(2036, 7, 1, 2);

        var numbers = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => Task.Run(() => TakeAsync(ProbeNumbers.Ticket), Ct)));

        numbers.Order(StringComparer.Ordinal).ShouldBe(Enumerable.Range(1, 20).Select(n => $"TK-203607-{n:D3}"));
    }

    [Fact]
    public async Task OutsideCommand_Rejected()
    {
        await using var work = _harness.Begin(_user);

        var thrown = await Should.ThrowAsync<InvalidOperationException>(
            async () => await work.Services.GetRequiredService<INumberGenerator>().NextAsync(ProbeNumbers.Ticket, ct: Ct));

        thrown.Message.ShouldContain("只能在命令里");
    }

    [Fact]
    public async Task BadPartition_Rejected()
    {
        At(2037, 1, 1, 2);

        // 命令里抛出的异常由管道转换成结果之外的异常，这里直接看分组键的校验
        var thrown = await Should.ThrowAsync<ArgumentException>(async () => await TakeAsync(ProbeNumbers.Review, "a-b"));

        thrown.Message.ShouldContain("分组键");
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private void At(int year, int month, int day, int hour) =>
        _harness.Time.SetUtcNow(new DateTimeOffset(year, month, day, hour, 0, 0, TimeSpan.Zero));

    private async Task<string> TakeAsync(string rule, string? partition = null)
    {
        await using var work = _harness.Begin(_user);
        return (await work.Sender.SendAsync(new TakeNumberCommand(rule, partition), Ct)).ShouldBeSuccess().Number;
    }

    private async Task SetLastValueAsync(string rule, string period, string partition, long value)
    {
        await using var command = database.DataSource.CreateCommand(
            """UPDATE "Probe"."NumberCounter" SET "LastValue" = @value WHERE "Rule" = @rule AND "PeriodKey" = @period AND "PartitionKey" = @partition""");
        command.Parameters.AddWithValue("value", value);
        command.Parameters.AddWithValue("rule", rule);
        command.Parameters.AddWithValue("period", period);
        command.Parameters.AddWithValue("partition", partition);
        (await command.ExecuteNonQueryAsync(Ct)).ShouldBe(1);
    }
}
