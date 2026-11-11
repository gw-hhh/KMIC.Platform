namespace Kmic.BuildingBlocks.Infrastructure.SecurityEvents;

/// <summary>
/// 安全事件配置
/// </summary>
public sealed class SecurityEventOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string Section = "Kmic:SecurityEvents";

    /// <summary>
    /// 等待写库的事件最多有多少条，满了以后新来的丢弃
    /// </summary>
    public int QueueCapacity { get; set; } = 10_000;

    /// <summary>
    /// 一次最多写多少条
    /// </summary>
    public int BatchSize { get; set; } = 200;

    /// <summary>
    /// 攒一批的最长等待时间
    /// </summary>
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(1);
}
