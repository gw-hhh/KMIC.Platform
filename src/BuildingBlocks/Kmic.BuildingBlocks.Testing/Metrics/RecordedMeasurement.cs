namespace Kmic.BuildingBlocks.Testing.Metrics;

/// <summary>单次指标度量记录。</summary>
/// <param name="Instrument">指标名称</param>
/// <param name="Value">度量值</param>
/// <param name="Tags">维度标签</param>
public sealed record RecordedMeasurement(string Instrument, double Value, IReadOnlyDictionary<string, string> Tags);
