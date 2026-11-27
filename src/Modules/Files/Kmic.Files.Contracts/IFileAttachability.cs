namespace Kmic.Files.Contracts;

/// <summary>查询可关联的文件，供 <see cref="IFileAttachments" /> 获取锁后以系统身份从主库调用（不使用缓存）</summary>
public interface IFileAttachability
{
    /// <summary>返回指定文件中可关联的文件快照，不可关联的不包含在内</summary>
    Task<IReadOnlyList<FileSnapshot>> CheckAsync(IReadOnlyCollection<Guid> fileIds, Guid uploaderId, CancellationToken ct);
}
