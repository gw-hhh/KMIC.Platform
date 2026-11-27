namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>错误附加元数据的标准键名。</summary>
public static class ErrorMetadataKeys
{
    /// <summary>当前版本号。</summary>
    public const string CurrentVersion = "currentVersion";

    /// <summary>标识是否可重试。</summary>
    public const string Retryable = "retryable";

    /// <summary>建议等待的重试秒数（仅用于 TooManyRequests）。</summary>
    public const string RetryAfterSeconds = "retryAfterSeconds";
}
