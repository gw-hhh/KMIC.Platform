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
}
