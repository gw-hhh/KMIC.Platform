using System.ComponentModel.DataAnnotations;

namespace Kmic.Operations.Application.ClientErrors;

/// <summary>
/// 前端错误上报的部署配置
/// </summary>
public sealed class ClientErrorOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Operations:ClientErrors";

    /// <summary>
    /// 单次请求的错误条数上限
    /// </summary>
    [Range(1, 200)]
    public int MaxPerRequest { get; init; } = 20;

    /// <summary>
    /// 每用户每分钟的上报请求数上限，超出返回 429，前端丢弃本批且不重试。按实例计数，仅用于防滥用，不保证多实例下的全局精确限额
    /// </summary>
    [Range(1, 10_000)]
    public int RequestsPerUserPerMinute { get; init; } = 30;
}
