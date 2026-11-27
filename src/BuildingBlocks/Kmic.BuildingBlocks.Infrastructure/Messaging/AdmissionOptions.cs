namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>容量保护配置（阈值应高于 RDS 自动扩容触发线，熔断仅作为最终兜底）</summary>
public sealed class AdmissionOptions
{
    /// <summary>采样间隔（Api 与 Worker 均运行）</summary>
    public TimeSpan SampleInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>软线阈值（已用空间占总容量比例）</summary>
    public double SoftRatio { get; set; } = 0.85;

    /// <summary>硬线阈值（已用空间占总容量比例）</summary>
    public double HardRatio { get; set; } = 0.95;

    /// <summary>触发软线的未完成消息量下限（字节），低于此值视为非消息积压引起</summary>
    public long MinUnfinishedBytes { get; set; } = 256L * 1024 * 1024;

    /// <summary>解除软线的未完成消息量低水位（字节），须连续两个采样周期不高于此值</summary>
    public long LowWaterBytes { get; set; } = 64L * 1024 * 1024;
}
