using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Files.Domain.Files;

/// <summary>
/// 存储问题的种类
/// </summary>
public sealed class StorageIssueKind : Enumeration<StorageIssueKind>
{
    /// <summary>
    /// 发现时当前对象缺失，但记录的对象版本仍可还原
    /// </summary>
    public static readonly StorageIssueKind RestorableMissing = new("RestorableMissing", "可以还原");

    /// <summary>
    /// 发现时对象缺失，未找到可还原的记录版本
    /// </summary>
    public static readonly StorageIssueKind LostMissing = new("LostMissing", "已经丢失");

    private StorageIssueKind(string code, string name)
        : base(code, name)
    {
    }
}
