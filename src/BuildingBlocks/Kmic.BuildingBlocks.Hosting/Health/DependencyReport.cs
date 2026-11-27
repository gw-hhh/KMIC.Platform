namespace Kmic.BuildingBlocks.Hosting.Health;

internal sealed record DependencyReport(string Status, double TotalDurationMs, IReadOnlyList<DependencyEntry> Entries);
