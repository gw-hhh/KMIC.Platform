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
    /// 一次请求最多带多少条错误
    /// </summary>
    [Range(1, 200)]
    public int MaxPerRequest { get; init; } = 20;

    /// <summary>
    /// 每个用户每分钟最多上报几次请求；超出时 429，前端丢弃不重试。按实例各自计数，是防刷的下限而不是精确限额
    /// </summary>
    [Range(1, 10_000)]
    public int RequestsPerUserPerMinute { get; init; } = 30;
}
