namespace Kmic.BuildingBlocks.Infrastructure.Observability;

/// <summary>
/// 量表读数
/// </summary>
/// <param name="Gauge">量表名称</param>
/// <param name="Value">值</param>
/// <param name="Tags">维度，只能是取值有限的内容</param>
public sealed record BacklogReading(string Gauge, double Value, IReadOnlyList<KeyValuePair<string, object?>> Tags);
