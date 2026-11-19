using Kmic.Files.Domain.Files;

namespace Kmic.Files.Application;

/// <summary>
/// Files 模块的查询入口
/// </summary>
public interface IFilesQueryDb
{
    /// <summary>
    /// 文件及其引用
    /// </summary>
    IQueryable<StoredFile> Files { get; }

    /// <summary>
    /// 对账发现的存储问题
    /// </summary>
    IQueryable<StorageIssue> StorageIssues { get; }
}
