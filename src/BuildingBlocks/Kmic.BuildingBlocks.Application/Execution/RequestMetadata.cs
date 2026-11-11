namespace Kmic.BuildingBlocks.Application.Execution;

/// <summary>
/// HTTP 请求带来的、与身份无关的元数据：幂等键，以及本次是否为重放
/// </summary>
/// <remarks>
/// 每个作用域一个；幂等键由接口层从 Idempotency-Key 请求头读取后设置，重放标记由幂等行为设置，接口层据此加响应头
/// </remarks>
public sealed class RequestMetadata
{
    /// <summary>
    /// 幂等键；非 HTTP 入口或请求没有带时为空
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// 本次响应是否为首次结果的重放
    /// </summary>
    public bool Replayed { get; set; }
}
