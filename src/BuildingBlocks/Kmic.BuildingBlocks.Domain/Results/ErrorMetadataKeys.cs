namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>
/// 错误附加数据的键
/// </summary>
public static class ErrorMetadataKeys
{
    /// <summary>
    /// 当前版本号
    /// </summary>
    public const string CurrentVersion = "currentVersion";

    /// <summary>
    /// 是否可以重试
    /// </summary>
    public const string Retryable = "retryable";
}
