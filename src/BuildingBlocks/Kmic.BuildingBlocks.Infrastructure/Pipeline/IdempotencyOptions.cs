namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

public sealed class IdempotencyOptions
{
    public const string Section = "Kmic:Idempotency";

    /// <summary>处理中记录的占用时长（处理进程崩溃时需等占用过期后才允许接管）</summary>
    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>幂等记录保留时长，过期后同一幂等键视为新请求</summary>
    public TimeSpan Retention { get; set; } = TimeSpan.FromHours(24);
}
