using System.ComponentModel.DataAnnotations;
using System.Net;
using Kmic.BuildingBlocks.Application.Time;

namespace Kmic.BuildingBlocks.Presentation.Limits;

/// <summary>接口层运行与限流部署配置（按单实例本地计数，多实例部署时整体上限按实例数倍增）</summary>
public sealed class KmicHttpOptions : IValidatableObject
{
    public const string Section = "Kmic:Http";

    /// <summary>同步请求超时时间（更耗时的操作应设计为后台作业）</summary>
    [TimeSpanRange("00:00:01", "01:00:00")]
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>匿名请求限流阈值（单 IP 每分钟最大请求数）</summary>
    [Range(1, 100_000)]
    public int AnonymousRequestsPerMinute { get; init; } = 30;

    /// <summary>已认证请求限流阈值（单用户每分钟最大请求数）</summary>
    [Range(1, 1_000_000)]
    public int AuthenticatedRequestsPerMinute { get; init; } = 600;

    /// <summary>单实例重型端点并发上限</summary>
    [Range(1, 1_000)]
    public int HeavyConcurrency { get; init; } = 4;

    /// <summary>默认请求体大小上限（字节）；上传等大负载端点须在端点内自行放宽</summary>
    [Range(1024, 100_000_000)]
    public long MaxRequestBodyBytes { get; init; } = 1_048_576;

    /// <summary>可信代理 CIDR 网段列表（如 10.0.0.0/16），为空时不采信转发头</summary>
    public IReadOnlyList<string> TrustedProxies { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var proxy in TrustedProxies.Where(p => !IPNetwork.TryParse(p, out _)))
        {
            yield return new ValidationResult($"{Section}:TrustedProxies 中的“{proxy}”不是有效的 CIDR 网段格式（示例：10.0.0.0/16）。", [nameof(TrustedProxies)]);
        }
    }
}
