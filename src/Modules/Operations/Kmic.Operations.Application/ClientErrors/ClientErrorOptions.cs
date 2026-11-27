using System.ComponentModel.DataAnnotations;

namespace Kmic.Operations.Application.ClientErrors;

/// <summary>前端错误上报配置。</summary>
public sealed class ClientErrorOptions
{
    public const string Section = "Operations:ClientErrors";

    /// <summary>单次请求的错误条数上限。</summary>
    [Range(1, 200)]
    public int MaxPerRequest { get; init; } = 20;

    /// <summary>单用户每分钟上报次数上限（各实例独立计数）。</summary>
    [Range(1, 10_000)]
    public int RequestsPerUserPerMinute { get; init; } = 30;
}
