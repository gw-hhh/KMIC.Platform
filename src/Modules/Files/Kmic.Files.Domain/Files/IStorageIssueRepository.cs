using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Files.Domain.Files;

/// <summary>
/// 存储问题的仓储
/// </summary>
public interface IStorageIssueRepository : IRepository<StorageIssue, StorageIssueId>
{
    /// <summary>
    /// 这些文件里，哪些已经有待处理的问题
    /// </summary>
    /// <param name="fileIds">要查的文件</param>
    /// <param name="ct">取消令牌</param>
    Task<IReadOnlySet<StoredFileId>> FindOpenFileIdsAsync(IReadOnlyCollection<StoredFileId> fileIds, CancellationToken ct);
}
