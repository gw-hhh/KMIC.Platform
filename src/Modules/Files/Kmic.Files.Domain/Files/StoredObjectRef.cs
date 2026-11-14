using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Files.Domain.Files;

/// <summary>
/// 文件在存储里的位置，对账用
/// </summary>
/// <remarks>
/// 不是领域概念，只是仓储给对账返回的查询结果
/// </remarks>
/// <param name="id">文件</param>
/// <param name="storageKey">对象的存储键</param>
/// <param name="objectVersionId">记录的对象版本，存储没有版本控制时为空</param>
public sealed class StoredObjectRef(StoredFileId id, string storageKey, string? objectVersionId)
{
    /// <summary>
    /// 文件
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
