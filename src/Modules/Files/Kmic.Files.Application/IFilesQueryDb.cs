using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application;

public interface IFilesQueryDb
{
    IQueryable<StoredFile> Files { get; }

    /// <summary>对账发现的存储问题</summary>
    IQueryable<StorageIssue> StorageIssues { get; }
}
