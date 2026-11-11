namespace Kmic.BuildingBlocks.Testing.Metrics;

/// <summary>
/// 一次指标记录
/// </summary>
/// <param name="Instrument">指标名</param>
/// <param name="Value">记录的值</param>
/// <param name="Tags">维度</param>
public sealed record RecordedMeasurement(string Instrument, double Value, IReadOnlyDictionary<string, string> Tags);
