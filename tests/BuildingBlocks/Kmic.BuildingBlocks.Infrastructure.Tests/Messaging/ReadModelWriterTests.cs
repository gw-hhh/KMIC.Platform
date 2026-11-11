using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Infrastructure.Tests.Probe;
using Kmic.BuildingBlocks.Testing.Database;
using Kmic.Echo.Probe;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Messaging;

/// <summary>
/// 每个测试用自己的 ID
/// </summary>
public sealed class ReadModelWriterTests(ClassDatabase<ProbeTemplate> database) : IClassFixture<ClassDatabase<ProbeTemplate>>, IAsyncDisposable
{
    private readonly ProbeHarness _harness = new(database.ConnectionString);
    private readonly Guid _id = Guid.CreateVersion7();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Live_OnlyHigherVersionWrites()
    {
        (await WithWriterAsync(w => w.UpsertAsync(Row(2, "二"), Ct))).ShouldBeTrue();

        (await WithWriterAsync(w => w.UpsertAsync(Row(1, "一"), Ct))).ShouldBeFalse();
        (await WithWriterAsync(w => w.UpsertAsync(Row(2, "二改"), Ct))).ShouldBeFalse();
        (await WithWriterAsync(w => w.UpsertAsync(Row(3, "三"), Ct))).ShouldBeTrue();

        var row = await ReadAsync();
        row.SourceVersion.ShouldBe(3);
        row.Title.ShouldBe("三");
        row.IsTerminal.ShouldBeFalse();
    }

    [Fact]
    public async Task PurgeThenLateSnapshot_NotRevived()
    {
        await WithWriterAsync(w => w.UpsertAsync(Row(1, "存在"), Ct));

        (await WithWriterAsync(w => w.PurgeAsync(Row(3, "", ReferenceStatus.Deleted), Ct))).ShouldBeTrue();
        (await WithWriterAsync(w => w.UpsertAsync(Row(2, "迟到的"), Ct))).ShouldBeFalse();
        (await WithWriterAsync(w => w.UpsertAsync(Row(9, "更新的也不行"), Ct))).ShouldBeFalse();
        (await WithWriterAsync(w => w.UpsertFromScanAsync(Row(9, "重建也不行"), Guid.CreateVersion7(), Ct))).ShouldBeFalse();

        var row = await ReadAsync();
        row.IsTerminal.ShouldBeTrue();
        row.TerminalReason.ShouldBe(ReadModelTerminalReason.PurgeEvent);
        row.TerminalAt.ShouldNotBeNull();
        row.SourceVersion.ShouldBe(3);
        row.Status.ShouldBe(ReferenceStatus.Deleted);
        row.Title.ShouldBe("存在");
    }

    [Fact]
    public async Task PurgeBeforeAnySnapshot_WritesTerminalRow()
    {
        (await WithWriterAsync(w => w.PurgeAsync(Row(5, "", ReferenceStatus.Deleted), Ct))).ShouldBeTrue();
        (await WithWriterAsync(w => w.UpsertAsync(Row(4, "迟到的"), Ct))).ShouldBeFalse();

        (await ReadAsync()).IsTerminal.ShouldBeTrue();
    }

    [Fact]
    public async Task Scan_SameVersionOverwrites_HigherLocalOnlyRecordsRun()
    {
        await WithWriterAsync(w => w.UpsertAsync(Row(2, "被改坏的"), Ct));
        var first = Guid.CreateVersion7();

        (await WithWriterAsync(w => w.UpsertFromScanAsync(Row(2, "修好的"), first, Ct))).ShouldBeTrue();
        var repaired = await ReadAsync();
        repaired.Title.ShouldBe("修好的");
        repaired.RebuildRunId.ShouldBe(first);

        await WithWriterAsync(w => w.UpsertAsync(Row(5, "实时的新版本"), Ct));
        var second = Guid.CreateVersion7();
        (await WithWriterAsync(w => w.UpsertFromScanAsync(Row(4, "扫描时还是旧的"), second, Ct))).ShouldBeTrue();

        var row = await ReadAsync();
        row.SourceVersion.ShouldBe(5);
        row.Title.ShouldBe("实时的新版本");
        row.RebuildRunId.ShouldBe(second);
    }

    public ValueTask DisposeAsync() => _harness.DisposeAsync();

    private TicketSummary Row(long version, string title, ReferenceStatus status = ReferenceStatus.Active) =>
        new() { Id = _id, SourceVersion = version, Status = status, Title = title };

    private async Task<bool> WithWriterAsync(Func<IReadModelWriter<TicketSummary>, Task<bool>> write)
    {
        await using var work = _harness.BeginAsSystem();
        return await write(work.Services.GetRequiredService<IReadModelWriter<TicketSummary>>());
    }

    private async Task<TicketSummary> ReadAsync()
    {
        await using var work = _harness.BeginAsSystem();
        return await work.Services.GetRequiredService<EchoQueryDbContext>().Tickets.SingleAsync(t => t.Id == _id, Ct);
    }
}
