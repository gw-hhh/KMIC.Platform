using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Files.Domain.Files;

/// <summary>
/// 存储问题的状态
/// </summary>
public sealed class StorageIssueStatus : Enumeration<StorageIssueStatus>
{
    /// <summary>
    /// 待处理
    /// </summary>
    public static readonly StorageIssueStatus Open = new("Open", "待处理");

    /// <summary>
    /// 已解决
    /// </summary>
    public static readonly StorageIssueStatus Resolved = new("Resolved", "已解决");

    private StorageIssueStatus(string code, string name)
        : base(code, name)
    {
    }
}
