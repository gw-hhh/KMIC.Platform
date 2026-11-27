namespace Kmic.BuildingBlocks.Hosting.Health;

internal sealed record DependencyEntry(string Name, string Status, string Description, double DurationMs);
