namespace Kmic.BuildingBlocks.Application.Behaviors;

/// <summary>
/// 中介者管道的配置
/// </summary>
public sealed class PipelineOptions
{
    /// <summary>
    /// 慢消息阈值：处理时间超过它的消息记警告日志，默认 1 秒
    /// </summary>
    public TimeSpan SlowMessageThreshold { get; set; } = TimeSpan.FromSeconds(1);
}
