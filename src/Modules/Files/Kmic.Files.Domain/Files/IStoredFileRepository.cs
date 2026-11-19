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
    /// 按 ID 升序，<paramref name="after" /> 为空时从头读
    /// </remarks>
    Task<IReadOnlyList<StoredObjectRef>> FindExpectedObjectsAsync(StoredFileId? after, DateTimeOffset uploadedBefore, int limit, CancellationToken ct);

    /// <summary>
    /// 查询指定状态下已到期的文件 ID
    /// </summary>
    /// <remarks>
    /// 只返回 ID，清理前要取锁再重读
    /// </remarks>
    Task<IReadOnlyList<StoredFileId>> FindExpiredAsync(StoredFileStatus status, DateTimeOffset now, int limit, CancellationToken ct);

    /// <summary>
    /// 查询超时未完成的上传
    /// </summary>
    Task<IReadOnlyList<StoredFileId>> FindStaleUploadsAsync(DateTimeOffset startedBefore, int limit, CancellationToken ct);

    /// <summary>
    /// 重置文件删除重试状态
    /// </summary>
    /// <returns>成功重置返回 true；记录不存在或状态不符合时返回 false</returns>
    Task<bool> ResetDeletionAsync(StoredFileId id, CancellationToken ct);
}
