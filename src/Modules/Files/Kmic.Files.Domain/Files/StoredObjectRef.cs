using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Domain.Files;

/// <summary>
/// 用于对账的文件存储位置
/// </summary>
public sealed class StoredObjectRef(StoredFileId id, string storageKey, string? objectVersionId)
{
    /// <summary>
    /// 文件 ID
    /// </summary>
    public StoredFileId Id { get; } = id;

    /// <summary>
    /// 对象的存储键
    /// </summary>
    public string StorageKey { get; } = storageKey;

    /// <summary>
    /// 记录的对象版本，存储没有版本控制时为空
    /// </summary>
    public string? ObjectVersionId { get; } = objectVersionId;
}
