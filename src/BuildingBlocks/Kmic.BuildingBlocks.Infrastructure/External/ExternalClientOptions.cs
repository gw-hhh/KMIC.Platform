namespace Kmic.BuildingBlocks.Infrastructure.External;

/// <summary>
/// 一个外部系统客户端的调用约束
/// </summary>
public sealed class ExternalClientOptions
{
    /// <summary>
    /// 一次调用的总超时，含重试
    /// </summary>
    public TimeSpan TotalTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 单次尝试的超时
    /// </summary>
    public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// 读请求（GET、HEAD）失败时是否重试，写请求从不重试
    /// </summary>
    public bool RetryReads { get; set; } = true;
}
