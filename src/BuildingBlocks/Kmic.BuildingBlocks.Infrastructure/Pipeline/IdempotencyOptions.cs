namespace Kmic.BuildingBlocks.Infrastructure.Pipeline;

/// <summary>
/// 幂等记录的租约与保留期（配置节 Kmic:Idempotency）
/// </summary>
public sealed class IdempotencyOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Kmic:Idempotency";

    /// <summary>
    /// 处理中的租约时长，默认 2 分钟；进程崩溃后到期即可被同键请求接管
    /// </summary>
    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// 记录保留期，默认 24 小时；之后同一个键按新请求处理
    /// </summary>
    public TimeSpan Retention { get; set; } = TimeSpan.FromHours(24);
}
