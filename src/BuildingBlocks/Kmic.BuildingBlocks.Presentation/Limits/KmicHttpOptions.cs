using System.ComponentModel.DataAnnotations;

namespace Kmic.BuildingBlocks.Presentation.Limits;

/// <summary>
/// 接口层的部署配置
/// </summary>
/// <remarks>
/// 内置限流按实例计数，多实例时总量是“实例数 × 限额”
/// </remarks>
public sealed class KmicHttpOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Kmic:Http";

    /// <summary>
    /// 请求超时，超过的操作应该做成后台作业
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:01", "01:00:00")]
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 没有登录的请求，每个 IP 每分钟的请求数
    /// </summary>
    [Range(1, 100_000)]
    public int AnonymousRequestsPerMinute { get; init; } = 30;

    /// <summary>
    /// 已登录的请求，每个用户每分钟的请求数
    /// </summary>
    [Range(1, 1_000_000)]
    public int AuthenticatedRequestsPerMinute { get; init; } = 600;

    /// <summary>
    /// 重型接口每个实例同时处理的请求数
    /// </summary>
    [Range(1, 1_000)]
    public int HeavyConcurrency { get; init; } = 4;
}
