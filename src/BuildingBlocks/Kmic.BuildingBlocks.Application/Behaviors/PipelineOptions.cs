namespace Kmic.BuildingBlocks.Application.Behaviors;

/// <summary>
/// 管道配置
/// </summary>
public sealed class PipelineOptions
{
    /// <summary>
    /// 慢请求阈值
    /// </summary>
    public TimeSpan SlowMessageThreshold { get; set; } = TimeSpan.FromSeconds(1);
}
