namespace Kmic.BuildingBlocks.Infrastructure.Observability;

/// <summary>积压指标量表定义</summary>
public sealed record BacklogGauge(string Name, string Unit, string Description);
