using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Files.Domain.Files;

/// <summary>
/// 一个业务对象对文件的引用
/// </summary>
public sealed class FileReference : Entity<FileReferenceId>
{
    private FileReference(FileReferenceId id, string ownerModule, string ownerType, Guid ownerId, DateTimeOffset attachedAt)
        : base(id)
    {
        OwnerModule = ownerModule;
        OwnerType = ownerType;
        OwnerId = ownerId;
        AttachedAt = attachedAt;
    }

    /// <summary>
    /// 引用它的业务模块
    /// </summary>
    public string OwnerModule { get; private set; }

    /// <summary>
    /// 业务对象的类型，如 Specimen
    /// </summary>
    public string OwnerType { get; private set; }

    /// <summary>
    /// 业务对象的 ID
    /// </summary>
    public Guid OwnerId { get; private set; }

    /// <summary>
    /// 关联的时间
    /// </summary>
    public DateTimeOffset AttachedAt { get; private set; }

    /// <summary>
    /// 是否属于指定业务对象
    /// </summary>
    public bool IsOwnedBy(string module, string type, Guid id) =>
        string.Equals(OwnerModule, module, StringComparison.Ordinal) && string.Equals(OwnerType, type, StringComparison.Ordinal) && OwnerId == id;

    internal static FileReference Create(string module, string type, Guid id, DateTimeOffset at) => new(FileReferenceId.New(), module, type, id, at);
}
