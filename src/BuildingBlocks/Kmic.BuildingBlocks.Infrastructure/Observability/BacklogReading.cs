namespace Kmic.BuildingBlocks.Infrastructure.Observability;

/// <summary>积压量表读数</summary>
public sealed record BacklogReading(string Gauge, double Value, IReadOnlyList<KeyValuePair<string, object?>> Tags);
