namespace Kmic.BuildingBlocks.Infrastructure.External;

/// <summary>外部客户端调用配置</summary>
public sealed class ExternalClientOptions
{
    /// <summary>单次调用总超时时间（含重试）</summary>
    public TimeSpan TotalTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>单次请求尝试超时时间</summary>
    public TimeSpan AttemptTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>读请求（GET、HEAD）失败时是否重试；写请求永不重试</summary>
    public bool RetryReads { get; set; } = true;
}
