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
    /// 每个用户每分钟的上报次数上限
    /// </summary>
    /// <remarks>
    /// 超限返回 429；各实例各自计数，只用于防滥用，不保证全局限额
    /// </remarks>
    [Range(1, 10_000)]
    public int RequestsPerUserPerMinute { get; init; } = 30;
}
