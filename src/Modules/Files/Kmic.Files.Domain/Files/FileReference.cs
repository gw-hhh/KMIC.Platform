using Kmic.BuildingBlocks.Domain.Entities;

namespace Kmic.Files.Domain.Files;

/// <summary>
/// 一个业务对象对文件的引用
/// </summary>
/// <remarks>
/// 引用登记只由关联、解除关联的请求消息维护，是业务模块里那份关联的最终一致的副本，只用来判断文件的生命周期：
/// 有引用就保留，没有引用了才进入保留期
/// </remarks>
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
    /// 是不是同一个业务对象的引用
    /// </summary>
    public bool IsOwnedBy(string module, string type, Guid id) =>
        string.Equals(OwnerModule, module, StringComparison.Ordinal) && string.Equals(OwnerType, type, StringComparison.Ordinal) && OwnerId == id;

    internal static FileReference Create(string module, string type, Guid id, DateTimeOffset at) => new(FileReferenceId.New(), module, type, id, at);
}
