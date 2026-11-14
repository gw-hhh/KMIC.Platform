using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Files.Domain.Files;

/// <summary>
/// 存储问题的种类
/// </summary>
public sealed class StorageIssueKind : Enumeration<StorageIssueKind>
{
    /// <summary>
    /// 可以还原：记录的对象版本还在，只是被删除标记盖住了
    /// </summary>
    public static readonly StorageIssueKind RestorableMissing = new("RestorableMissing", "可以还原");

    /// <summary>
    /// 已经丢失：记录的对象版本也找不到了
    /// </summary>
    public static readonly StorageIssueKind LostMissing = new("LostMissing", "已经丢失");

    private StorageIssueKind(string code, string name)
        : base(code, name)
    {
    }
}
