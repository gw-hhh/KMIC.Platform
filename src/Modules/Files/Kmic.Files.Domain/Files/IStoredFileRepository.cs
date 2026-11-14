using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Files.Domain.Files;

/// <summary>
/// 文件的仓储
/// </summary>
public interface IStoredFileRepository : IRepository<StoredFile, StoredFileId>
{
    /// <summary>
    /// 指定文件 ID 是否已登记
    /// </summary>
    Task<bool> ExistsAsync(StoredFileId id, CancellationToken ct);

    /// <summary>
    /// 查询指定 ID 中已登记的文件 ID
    /// </summary>
    Task<IReadOnlySet<StoredFileId>> FindExistingAsync(IReadOnlyCollection<StoredFileId> ids, CancellationToken ct);

    /// <summary>
    /// 分页查询应有存储对象的文件
    /// </summary>
    /// <remarks>
    /// 仅查询可用、已关联或已解除关联的文件，UploadedAt 不晚于 <paramref name="uploadedBefore" />。
    /// 按 ID 升序，<paramref name="after" /> 为上一页最后一个文件 ID，首页为空。只返回 ID 和存储位置，不加载聚合
    /// </remarks>
    Task<IReadOnlyList<StoredObjectRef>> FindExpectedObjectsAsync(StoredFileId? after, DateTimeOffset uploadedBefore, int limit, CancellationToken ct);

    /// <summary>
    /// 查询指定状态下已到期的文件 ID
    /// </summary>
    /// <remarks>
    /// 到期时间不晚于 <paramref name="now" />，按到期时间升序。
    /// 只返回 ID，避免清理前读入旧状态；清理可用或已解除关联的文件时，须先取锁并探查关联消息，再重读文件
    /// </remarks>
    Task<IReadOnlyList<StoredFileId>> FindExpiredAsync(StoredFileStatus status, DateTimeOffset now, int limit, CancellationToken ct);

    /// <summary>
    /// 查询超时未完成的上传
    /// </summary>
    /// <remarks>
    /// 状态仍为上传中，上传开始时间不晚于 <paramref name="startedBefore" />，按开始时间升序
    /// </remarks>
    Task<IReadOnlyList<StoredFileId>> FindStaleUploadsAsync(DateTimeOffset startedBefore, int limit, CancellationToken ct);

    /// <summary>
    /// 重置文件删除重试
    /// </summary>
    /// <remarks>
    /// 清零尝试次数，清除退避时间和领取信息，使文件可重新领取。
    /// 仅适用于删除中，或已彻底删除且尚未登记对象删除完成的文件
    /// </remarks>
    /// <returns>成功重置返回 true；记录不存在或状态不符合时返回 false</returns>
    Task<bool> ResetDeletionAsync(StoredFileId id, CancellationToken ct);
}
