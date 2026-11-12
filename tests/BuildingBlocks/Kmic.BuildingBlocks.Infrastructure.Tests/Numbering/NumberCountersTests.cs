using Kmic.BuildingBlocks.Application.Numbering;
using Kmic.BuildingBlocks.Infrastructure.Numbering;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;
using Kmic.BuildingBlocks.Testing.Assertions;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.BuildingBlocks.Testing.Identity;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Numbering;

/// <summary>
/// 导入旧数据之后抬高计数。每个测试用自己的年份，同一个库里的计数互不影响。Probe 的表在 Probe schema 里
/// </summary>
public sealed class NumberCountersTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private const string Schema = "Probe";

    private static readonly TestOrgTree _org = TestOrgTree.Create(t => t.Unit("总部"));
    private static readonly TestIdentity _user = TestIdentity.User("甲").InTree(_org).InOrgUnit(_org["总部"]);
    private static readonly NumberRuleCatalog _rules = NumberRuleCatalog.FromAssemblies([typeof(ProbeNumbers).Assembly]);

    private readonly ProbeHarness _harness = new(database.ConnectionString);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task NoCounterYet_Created_NextNumberContinuesFromIt()
    {
        At(2040, 3, 10);

        (await EnsureAsync(ProbeNumbers.Ticket, new DateOnly(2040, 3, 1), null, 57)).ShouldBe(57);

        (await TakeAsync(ProbeNumbers.Ticket)).ShouldBe("TK-204003-058");
    }

    [Fact]
    public async Task LowerValue_NeverMovesTheCounterBack()
    {
        At(2041, 3, 10);
        (await TakeAsync(ProbeNumbers.Ticket)).ShouldBe("TK-204103-001");
        (await TakeAsync(ProbeNumbers.Ticket)).ShouldBe("TK-204103-002");

        (await EnsureAsync(ProbeNumbers.Ticket, new DateOnly(2041, 3, 15), null, 1)).ShouldBe(2);

        (await TakeAsync(ProbeNumbers.Ticket)).ShouldBe("TK-204103-003");
    }

    [Fact]
    public async Task RepeatedWithSameValue_IsHarmless()
    {
        At(2042, 3, 10);

        await EnsureAsync(ProbeNumbers.Ticket, new DateOnly(2042, 3, 1), null, 9);
        await EnsureAsync(ProbeNumbers.Ticket, new DateOnly(2042, 3, 1), null, 9);

        (await TakeAsync(ProbeNumbers.Ticket)).ShouldBe("TK-204203-010");
    }

    [Fact]
    public async Task OnlyTheGivenPeriodAndPartition_AreRaised()
    {
        At(2043, 6, 1);

        await EnsureAsync(ProbeNumbers.Review, new DateOnly(2043, 2, 1), "A", 40);

        (await TakeAsync(ProbeNumbers.Review, "A")).ShouldBe("RV-43-A-41");
        (await TakeAsync(ProbeNumbers.Review, "B")).ShouldBe("RV-43-B-01");
    }

    [Fact]
    public async Task InvalidArguments_Rejected()
    {
        var ticket = _rules.All.Single(r => string.Equals(r.Name, ProbeNumbers.Ticket, StringComparison.Ordinal));

        await Should.ThrowAsync<ArgumentException>(() => EnsureAsync(ProbeNumbers.Ticket, new DateOnly(2044, 1, 1), null, 0));
        await Should.ThrowAsync<ArgumentException>(() => EnsureAsync(ProbeNumbers.Ticket, new DateOnly(2044, 1, 1), "A", 5));
        await Should.ThrowAsync<ArgumentException>(() => EnsureAsync(ProbeNumbers.Review, new DateOnly(2044, 1, 1), null, 5));
        await Should.ThrowAsync<ArgumentException>(() => NumberCounters.EnsureAtLeastAsync(
            database.DataSource, "Probe\"; DROP TABLE x;--", ticket, new DateOnly(2044, 1, 1), null, 5, _harness.Time, Ct));
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private void At(int year, int month, int day) =>
        _harness.Time.SetUtcNow(new DateTimeOffset(year, month, day, 2, 0, 0, TimeSpan.Zero));

    private Task<long> EnsureAsync(string rule, DateOnly date, string? partition, long value) =>
        NumberCounters.EnsureAtLeastAsync(
            database.DataSource, Schema, _rules.All.Single(r => string.Equals(r.Name, rule, StringComparison.Ordinal)), date, partition, value, _harness.Time, Ct);

    private async Task<string> TakeAsync(string rule, string? partition = null)
    {
        await using var work = _harness.Begin(_user);
        return (await work.Sender.SendAsync(new TakeNumberCommand(rule, partition), Ct)).ShouldBeSuccess().Number;
    }
}
