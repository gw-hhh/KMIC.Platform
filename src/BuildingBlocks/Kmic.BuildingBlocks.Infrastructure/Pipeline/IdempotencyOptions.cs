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
    /// 租约时长
    /// </summary>
    /// <remarks>
    /// 进程崩溃后，租约到期才能被同键请求接管
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
