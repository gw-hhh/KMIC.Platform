using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Domain.Files;

/// <summary>用于对账的文件存储位置</summary>
public sealed class StoredObjectRef(StoredFileId id, string storageKey, string? objectVersionId)
{
    public StoredFileId Id { get; } = id;

    public string StorageKey { get; } = storageKey;

    /// <summary>记录的对象版本，存储未开启版本控制时为空</summary>
    public string? ObjectVersionId { get; } = objectVersionId;
}
