namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 容量保护配置
/// </summary>
/// <remarks>
/// 软线、硬线比例要高于 RDS 存储自动扩容的触发线，让扩容先起作用，熔断只是最后手段
/// </remarks>
public sealed class AdmissionOptions
{
    /// <summary>
    /// 采样间隔，Api 与 Worker 角色都运行
    /// </summary>
    public TimeSpan SampleInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 软线：已用空间占容量的比例
    /// </summary>
    public double SoftRatio { get; set; } = 0.85;

    /// <summary>
    /// 硬线：已用空间占容量的比例
    /// </summary>
    public double HardRatio { get; set; } = 0.95;

    /// <summary>
    /// 软线要求的未完成消息量下限（字节），低于它说明增长不是队列造成的
    /// </summary>
    public long MinUnfinishedBytes { get; set; } = 256L * 1024 * 1024;

    /// <summary>
    /// 解除软线要求的未完成消息量上限（字节），要连续两个周期不高于它
    /// </summary>
    public long LowWaterBytes { get; set; } = 64L * 1024 * 1024;
}
