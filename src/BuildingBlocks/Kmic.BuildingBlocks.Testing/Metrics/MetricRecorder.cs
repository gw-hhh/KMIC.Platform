using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Kmic.BuildingBlocks.Testing.Metrics;

/// <summary>
/// 记录一个指标工厂创建的所有指标，供测试断言
/// </summary>
/// <remarks>
/// 按工厂过滤：同一个进程里并行的其他测试用自己的工厂，互不干扰
/// </remarks>
public sealed class MetricRecorder : IDisposable
{
    private readonly MeterListener _listener = new();
    private readonly ConcurrentQueue<RecordedMeasurement> _measurements = new();

    /// <summary>
    /// 开始记录这个工厂创建的指标
    /// </summary>
    public MetricRecorder(IMeterFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (ReferenceEquals(instrument.Meter.Scope, factory))
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        _listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) => Add(instrument, value, tags));
        _listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) => Add(instrument, value, tags));
        _listener.Start();
    }

    /// <summary>
    /// 全部记录，按发生顺序
    /// </summary>
    public IReadOnlyCollection<RecordedMeasurement> All => _measurements;

    /// <summary>
    /// 读取一遍所有量表（ObservableGauge 这类只在被读取时才产生数值），结果加入记录
    /// </summary>
    public void Collect() => _listener.RecordObservableInstruments();

    /// <summary>
    /// 清空已有的记录，从头观察下一段操作
    /// </summary>
    public void Clear() => _measurements.Clear();

    /// <summary>
    /// 某个指标的记录
    /// </summary>
    public IReadOnlyList<RecordedMeasurement> Of(string instrument) =>
        [.. _measurements.Where(m => string.Equals(m.Instrument, instrument, StringComparison.Ordinal))];

    /// <inheritdoc />
    public void Dispose() => _listener.Dispose();

    private void Add(Instrument instrument, double value, ReadOnlySpan<KeyValuePair<string, object?>> tags) =>
        _measurements.Enqueue(new RecordedMeasurement(
            instrument.Name,
            value,
            tags.ToArray().ToDictionary(t => t.Key, t => t.Value?.ToString() ?? "", StringComparer.Ordinal)));
}
