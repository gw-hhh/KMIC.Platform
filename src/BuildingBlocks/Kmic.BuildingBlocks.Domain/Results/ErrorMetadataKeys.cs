namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>
/// Error.Metadata 中框架使用的键；接口层只输出白名单中的键
/// </summary>
public static class ErrorMetadataKeys
{
    /// <summary>
    /// 并发冲突时数据库中的最新并发版本
    /// </summary>
    public const string CurrentVersion = "currentVersion";
}
