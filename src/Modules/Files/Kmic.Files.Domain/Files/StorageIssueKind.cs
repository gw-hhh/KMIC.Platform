using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Files.Domain.Files;

public sealed class StorageIssueKind : Enumeration<StorageIssueKind>
{
    /// <summary>当前对象缺失，但记录的对象版本仍可还原</summary>
    public static readonly StorageIssueKind RestorableMissing = new("RestorableMissing", "可还原");

    /// <summary>对象缺失且未找到可还原的记录版本</summary>
    public static readonly StorageIssueKind LostMissing = new("LostMissing", "已丢失");

    private StorageIssueKind(string code, string name)
        : base(code, name)
    {
    }
}
