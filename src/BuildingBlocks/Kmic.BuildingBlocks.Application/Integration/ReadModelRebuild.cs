using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Contracts.Snapshots;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.BuildingBlocks.Application.Integration;

internal sealed class ReadModelRebuild<TSource, TSnapshot, TRow>(Func<SnapshotItem<TSnapshot>, TRow> toRow) : IReadModelRebuild
    where TSource : class, ISnapshotSource<TSnapshot>
    where TRow : class, IReadModel
{
    public Type ReadModel => typeof(TRow);

    public Type Source => typeof(TSource);

    public async Task<RebuildPage> GetPageAsync(IServiceProvider services, Guid? after, int size, CancellationToken ct)
    {
        var page = await services.GetRequiredService<TSource>().GetPageAsync(after, size, ct);
        return new RebuildPage([.. page.Items.Select(toRow)], page.Next);
    }

    public async Task<IReadOnlyList<IReadModel>> GetRowsAsync(IServiceProvider services, IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        var found = await services.GetRequiredService<TSource>().GetByIdsAsync(ids, ct);
        var rows = new List<IReadModel>(found.Count);
        foreach (var (id, item) in found)
        {
            var row = toRow(item);
            if (row.Id != id)
            {
                throw new InvalidOperationException($"{typeof(TRow).Name} 的映射给出的 ID 是 {row.Id}，快照来源返回的是 {id}，两者要一致。");
            }

            rows.Add(row);
        }

        return rows;
    }

    public Task<bool> ScanAsync(IServiceProvider services, IReadModel row, Guid runId, CancellationToken ct) =>
        services.GetRequiredService<IReadModelWriter<TRow>>().UpsertFromScanAsync((TRow)row, runId, ct);

    public Task<long> CountAsync(IServiceProvider services, CancellationToken ct) =>
        Maintenance(services).CountAsync(ct);

    public Task<IReadOnlyList<ReadModelCandidate>> ListCandidatesAsync(IServiceProvider services, Guid runId, Guid? after, int size, CancellationToken ct) =>
        Maintenance(services).ListCandidatesAsync(runId, after, size, ct);

    public Task<int> MarkMissingAsync(IServiceProvider services, IReadOnlyCollection<ReadModelCandidate> rows, CancellationToken ct) =>
        Maintenance(services).MarkMissingAsync(rows, ct);

    private static IReadModelMaintenance<TRow> Maintenance(IServiceProvider services) =>
        services.GetRequiredService<IReadModelMaintenance<TRow>>();
}
