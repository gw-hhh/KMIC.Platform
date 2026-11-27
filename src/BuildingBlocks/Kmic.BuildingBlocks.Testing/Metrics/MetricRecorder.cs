using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Kmic.BuildingBlocks.Testing.Metrics;

/// <summary>记录指定指标工厂创建的所有度量指标，供测试断言。</summary>
public sealed class MetricRecorder : IDisposable
{
    private readonly MeterListener _listener = new();
    private readonly ConcurrentQueue<RecordedMeasurement> _measurements = new();

    /// <summary>开始监听并记录指定指标工厂创建的指标。</summary>
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

    /// <summary>按采集顺序返回全部度量记录。</summary>
    public IReadOnlyCollection<RecordedMeasurement> All => _measurements;

    /// <summary>采集所有 ObservableGauge 指标并加入记录。</summary>
    public void Collect() => _listener.RecordObservableInstruments();

    /// <summary>清空已有记录，以便重新观察后续操作。</summary>
    public void Clear() => _measurements.Clear();

    /// <summary>获取指定指标名称的所有度量记录。</summary>
    public IReadOnlyList<RecordedMeasurement> Of(string instrument) =>
        [.. _measurements.Where(m => string.Equals(m.Instrument, instrument, StringComparison.Ordinal))];

    public void Dispose() => _listener.Dispose();

    private void Add(Instrument instrument, double value, ReadOnlySpan<KeyValuePair<string, object?>> tags) =>
        _measurements.Enqueue(new RecordedMeasurement(
            instrument.Name,
            value,
            tags.ToArray().ToDictionary(t => t.Key, t => t.Value?.ToString() ?? "", StringComparer.Ordinal)));
}
