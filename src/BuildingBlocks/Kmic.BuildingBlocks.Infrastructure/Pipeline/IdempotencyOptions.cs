namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 幂等配置
/// </summary>
public sealed class IdempotencyOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string Section = "Kmic:Idempotency";

    /// <summary>
    /// 处理中记录的占用时长
    /// </summary>
    /// <remarks>
    /// 进程崩溃后，要等占用到期，同一个键的请求才能接管
    /// </remarks>
    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// 记录保留时长
    /// </summary>
    /// <remarks>
    /// 过了保留时长，同一个键按新请求处理
    /// </remarks>
    public TimeSpan Retention { get; set; } = TimeSpan.FromHours(24);
}
