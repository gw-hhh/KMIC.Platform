using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Files.Domain.Files;

/// <summary>
/// 文件的仓储
/// </summary>
public interface IStoredFileRepository : IRepository<StoredFile, StoredFileId>
{
    /// <summary>
    /// 这个 ID 的文件是不是已经登记了
    /// </summary>
    /// <param name="id">文件 ID</param>
    /// <param name="ct">取消令牌</param>
    Task<bool> ExistsAsync(StoredFileId id, CancellationToken ct);

    /// <summary>
    /// 找出到期的文件：处于某个状态、到期时间不晚于现在，最早到期的在前
    /// </summary>
    /// <remarks>
    /// 只返回 ID，不加载聚合：清理要先取锁、探查消息，之后才读文件，不能用这里读到的旧状态
    /// </remarks>
    /// <param name="status">要找的状态：临时的 Available、已解除关联的 Detached、被拒绝的 Rejected</param>
    /// <param name="now">现在</param>
    /// <param name="limit">最多几个</param>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlyList<StoredFileId>> FindExpiredAsync(StoredFileStatus status, DateTimeOffset now, int limit, CancellationToken ct);

    /// <summary>
    /// 找出上传中断的文件：一直处于“上传中”，开始得不晚于 <paramref name="startedBefore" />
    /// </summary>
    /// <param name="startedBefore">上传开始的时间不晚于它</param>
    /// <param name="limit">最多几个</param>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlyList<StoredFileId>> FindStaleUploadsAsync(DateTimeOffset startedBefore, int limit, CancellationToken ct);

    /// <summary>
    /// 把一个删除失败太多次的文件放回删除队列：清零重试次数和退避，让删除工作器重新领取
    /// </summary>
    /// <param name="id">文件 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>这个文件现在在删除队列里（删除中，或已彻底删除而对象还没删）并且被重置了，否则 false</returns>
    Task<bool> ResetDeletionAsync(StoredFileId id, CancellationToken ct);
}
