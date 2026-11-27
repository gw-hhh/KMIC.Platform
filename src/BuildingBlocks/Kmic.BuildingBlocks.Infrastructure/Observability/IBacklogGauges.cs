namespace Kmic.BuildingBlocks.Infrastructure.Observability;

/// <summary>模块自定义积压量表（查询应使用后台连接池，且仅统计未完成记录）</summary>
public interface IBacklogGauges
{
    /// <summary>所属 Meter 名称，如 Kmic.Notifications</summary>
    string MeterName { get; }

    IReadOnlyList<BacklogGauge> Gauges { get; }

    Task<IReadOnlyList<BacklogReading>> ReadAsync(CancellationToken ct);
}
