namespace Spike16.Tests;

internal sealed record RebuildOptions
{
    public int MaxMissingCount { get; init; } = 100;

    public double MaxMissingRatio { get; init; } = 0.01;

    public Func<Guid?, Task<IReadOnlyList<Snapshot>>>? GetPage { get; init; }

    public Func<IReadOnlyCollection<Guid>, Task<IReadOnlyDictionary<Guid, Snapshot>>>? GetByIds { get; init; }

    public Func<Task>? AfterScan { get; init; }

    public Func<Task>? BeforeConfirmMissing { get; init; }
}
