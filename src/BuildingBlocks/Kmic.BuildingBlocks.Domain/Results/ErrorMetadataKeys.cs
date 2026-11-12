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

    /// <summary>
    /// 还要等多少秒才能重试，只用于“请求过于频繁”
    /// </summary>
    public const string RetryAfterSeconds = "retryAfterSeconds";
}
