namespace Kmic.BuildingBlocks.Application.Execution;

/// <summary>
/// 请求元数据
/// </summary>
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
