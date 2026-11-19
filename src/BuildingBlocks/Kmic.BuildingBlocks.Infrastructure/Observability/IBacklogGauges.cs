namespace Kmic.BuildingBlocks.Infrastructure.Observability;

/// <summary>
/// 模块自己的积压量表
/// </summary>
/// <remarks>
/// 每个采样间隔读一次，Api 和 Worker 实例都会读，所以查询要走后台池、只读未完成的行
/// </remarks>
public interface IBacklogGauges
{
    /// <summary>
    /// 量表所属 Meter 的名称，如 Kmic.Notifications
    /// </summary>
    string MeterName { get; }

    /// <summary>
    /// 量表定义
    /// </summary>
    IReadOnlyList<BacklogGauge> Gauges { get; }

    /// <summary>
    /// 读取各量表的当前值
    /// </summary>
    Task<IReadOnlyList<BacklogReading>> ReadAsync(CancellationToken ct);
}
