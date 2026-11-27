using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Infrastructure.Observability;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Files.Application;
using Kmic.Files.Domain.Files;
using Kmic.Files.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Files.Infrastructure.Storage;

internal sealed class StorageIssueGauges(IQueryScopeFactory queries) : IBacklogGauges
{
    private const string RestorePending = "kmic.files.restore_pending";

    public string MeterName => FilesMetrics.MeterName;

    public IReadOnlyList<BacklogGauge> Gauges { get; } =
    [
        new(RestorePending, "{file}", "对象丢失但可从历史版本还原的待处理文件数"),
    ];

    public async Task<IReadOnlyList<BacklogReading>> ReadAsync(CancellationToken ct)
    {
        var count = await queries.RunAsync<FilesQueryDbContext, int>(
            CallOrigin.BackgroundJob,
            (db, token) => db.Set<StorageIssue>().CountAsync(i => i.Kind == StorageIssueKind.RestorableMissing && i.Status == StorageIssueStatus.Open, token),
            ct);
        return [new BacklogReading(RestorePending, count, [])];
    }
}
