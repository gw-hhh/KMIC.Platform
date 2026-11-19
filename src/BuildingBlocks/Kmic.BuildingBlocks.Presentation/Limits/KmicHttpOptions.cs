using System.ComponentModel.DataAnnotations;
using System.Net;
using Kmic.BuildingBlocks.Application.Time;

namespace Kmic.BuildingBlocks.Presentation.Limits;

/// <summary>
/// 接口层的部署配置
/// </summary>
/// <remarks>
/// 内置限流按实例计数，多实例时总量是实例数 × 限额
/// </remarks>
public sealed class KmicHttpOptions : IValidatableObject
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Kmic:Http";

    /// <summary>
    /// 请求超时，超过的操作应该做成后台作业
    /// </summary>
    [TimeSpanRange("00:00:01", "01:00:00")]
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

    /// <summary>
    /// 请求体大小上限（字节），要收更大请求体的端点（如上传）自己在请求里放宽
    /// </summary>
    [Range(1024, 100_000_000)]
    public long MaxRequestBodyBytes { get; init; } = 1_048_576;

    /// <summary>
    /// 可信代理（负载均衡）所在的网段，如 10.0.0.0/16；只有来自这些网段的转发头才被采信，为空表示不处理转发头
    /// </summary>
    public IReadOnlyList<string> TrustedProxies { get; init; } = [];

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var proxy in TrustedProxies.Where(p => !IPNetwork.TryParse(p, out _)))
        {
            yield return new ValidationResult($"{Section}:TrustedProxies 里的“{proxy}”不是 CIDR 网段，应写成 10.0.0.0/16 这样。", [nameof(TrustedProxies)]);
        }
    }
}
