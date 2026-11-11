namespace Kmic.BuildingBlocks.Application.Execution;

/// <summary>
/// 请求元数据
/// </summary>
/// <remarks>
/// 幂等键由接口层从 Idempotency-Key 请求头读取，重放标记由幂等处理设置
/// </remarks>
public sealed class RequestMetadata
{
    /// <summary>
    /// 幂等键
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>
    /// 是否为重放结果
    /// </summary>
    public bool Replayed { get; set; }
}
