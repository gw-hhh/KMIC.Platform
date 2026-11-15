namespace Kmic.Files.Contracts;

/// <summary>
/// 查询可关联的文件，供 <see cref="IFileAttachments" /> 获取锁后以系统身份调用
/// </summary>
/// <remarks>
/// 从主实例读取，不使用缓存。支持关联人上传的可用文件，以及已解除关联的文件；关联人为系统账号时，还可关联无上传人的可用文件。
/// 不按到期时间拒绝关联；尚未进入删除队列的过期文件仍可关联，通过咨询锁避免与清理冲突
/// </remarks>
public interface IFileAttachability
{
    /// <summary>
    /// 返回指定文件中可关联的文件快照
    /// </summary>
    /// <param name="fileIds">文件 ID</param>
    /// <param name="uploaderId">关联人 ID；用于校验可用文件的上传人，已解除关联的文件不受此限制</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>每个能关联的文件一份快照，不能关联的不在结果里</returns>
    Task<IReadOnlyList<FileSnapshot>> CheckAsync(IReadOnlyCollection<Guid> fileIds, Guid uploaderId, CancellationToken ct);
}
