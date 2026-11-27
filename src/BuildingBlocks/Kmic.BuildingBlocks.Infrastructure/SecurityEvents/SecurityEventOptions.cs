namespace Kmic.BuildingBlocks.Infrastructure.SecurityEvents;

public sealed class SecurityEventOptions
{
    public const string Section = "Kmic:SecurityEvents";

    /// <summary>待写库队列最大容量，超出时新事件将被丢弃</summary>
    public int QueueCapacity { get; set; } = 10_000;

    /// <summary>单次批量落库的最大事件数</summary>
    public int BatchSize { get; set; } = 200;

    /// <summary>凑批等待的最长时间</summary>
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(1);
}
