using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Files.Domain.Files;

public sealed class StorageIssueStatus : Enumeration<StorageIssueStatus>
{
    public static readonly StorageIssueStatus Open = new("Open", "待处理");

    public static readonly StorageIssueStatus Resolved = new("Resolved", "已解决");

    private StorageIssueStatus(string code, string name)
        : base(code, name)
    {
    }
}
