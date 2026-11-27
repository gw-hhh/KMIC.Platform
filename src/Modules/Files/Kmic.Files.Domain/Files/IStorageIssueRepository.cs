using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Files.Domain.Files;

public interface IStorageIssueRepository : IRepository<StorageIssue, StorageIssueId>
{
    /// <summary>查询指定文件中已有待处理问题的文件 ID</summary>
    Task<IReadOnlySet<StoredFileId>> FindOpenFileIdsAsync(IReadOnlyCollection<StoredFileId> fileIds, CancellationToken ct);
}
