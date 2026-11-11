using Kmic.BuildingBlocks.Application.Integration;
using Kmic.BuildingBlocks.Contracts.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using static Kmic.BuildingBlocks.Application.Tests.Integration.IntegrationMessageSamples;

namespace Kmic.BuildingBlocks.Application.Tests.Integration;

public sealed class ReadModelRebuildTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task GetPage_MapsEverySnapshotToARow()
    {
        var first = Guid.CreateVersion7();
        var second = Guid.CreateVersion7();
        var source = new FakeSource(items: [new SnapshotItem<ThingChanged>(new ThingChanged(first), 3), new SnapshotItem<ThingChanged>(new ThingChanged(second), 5)]);

        var page = await NewRebuild().GetPageAsync(Services(source), after: null, size: 10, Ct);

        page.Rows.Select(r => (r.Id, r.SourceVersion)).ShouldBe([(first, 3L), (second, 5L)]);
        page.Next.ShouldBe(second);
    }

    [Fact]
    public async Task GetRows_RowIdDiffersFromSourceKey_Throws()
    {
        var key = Guid.CreateVersion7();
        var source = new FakeSource(byId: new Dictionary<Guid, SnapshotItem<ThingChanged>> { [key] = new(new ThingChanged(Guid.CreateVersion7()), 1) });

        var thrown = await Should.ThrowAsync<InvalidOperationException>(() => NewRebuild().GetRowsAsync(Services(source), [key], Ct));

        thrown.Message.ShouldContain("ThingRow");
    }

    private static IReadModelRebuild NewRebuild()
    {
        var options = new SubscriptionOptions().RebuildWith<IThingSnapshotSource, ThingChanged, ThingRow>(item => new ThingRow(item.Snapshot.Id) { SourceVersion = item.SourceVersion });
        return options.Rebuild.ShouldNotBeNull();
    }

    private static ServiceProvider Services(FakeSource source) =>
        new ServiceCollection().AddSingleton<IThingSnapshotSource>(source).BuildServiceProvider();

    private sealed class FakeSource(IReadOnlyList<SnapshotItem<ThingChanged>>? items = null, IReadOnlyDictionary<Guid, SnapshotItem<ThingChanged>>? byId = null) : IThingSnapshotSource
    {
        public Task<SnapshotPage<ThingChanged>> GetPageAsync(Guid? after, int size, CancellationToken ct) =>
            Task.FromResult(new SnapshotPage<ThingChanged>(items ?? [], items?.Count > 0 ? items[^1].Snapshot.Id : null));

        public Task<IReadOnlyDictionary<Guid, SnapshotItem<ThingChanged>>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct) =>
            Task.FromResult(byId ?? new Dictionary<Guid, SnapshotItem<ThingChanged>>());
    }
}
