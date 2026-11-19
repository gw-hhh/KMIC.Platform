namespace Kmic.Files.Contracts;

/// <summary>
/// 查询可关联的文件，供 <see cref="IFileAttachments" /> 获取锁后以系统身份调用
/// </summary>
/// <remarks>
/// 从主库读，不用缓存
/// </remarks>
public interface IFileAttachability
{
    /// <summary>
    /// 返回指定文件中可关联的文件快照
    /// </summary>
    /// <returns>每个能关联的文件一份快照，不能关联的不在结果里</returns>
    Task<IReadOnlyList<FileSnapshot>> CheckAsync(IReadOnlyCollection<Guid> fileIds, Guid uploaderId, CancellationToken ct);
}
