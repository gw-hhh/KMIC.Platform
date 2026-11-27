namespace Kmic.BuildingBlocks.Application.Behaviors;

/// <summary>请求管道配置。</summary>
public sealed class PipelineOptions
{
    public const string Section = "Kmic:Pipeline";

    /// <summary>慢请求告警阈值。</summary>
    public TimeSpan SlowMessageThreshold { get; set; } = TimeSpan.FromSeconds(1);
}
