namespace Kmic.Files.Contracts;

/// <summary>
/// 检查一批文件现在能不能被关联：给 <see cref="IFileAttachments" /> 在互斥协议的第 2 步用，只允许系统身份调用
/// </summary>
/// <remarks>
/// 读主实例的新快照，不经任何缓存。条件：状态是“可用”且是这个人上传的，或者是可以重新关联的“已解除关联”；
/// 不检查到期时间：已过期但还没进入删除队列的文件仍可关联，互斥由咨询锁保证，不靠时钟
/// </remarks>
public interface IFileAttachability
{
    /// <summary>
    /// 检查一批文件
    /// </summary>
    /// <param name="fileIds">文件 ID</param>
    /// <param name="uploaderId">关联的人：“可用”的文件必须是他上传的；已经关联过、重新关联的“已解除关联”文件不要求</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>每个能关联的文件一份快照，不能关联的不在结果里</returns>
    Task<IReadOnlyList<FileSnapshot>> CheckAsync(IReadOnlyCollection<Guid> fileIds, Guid uploaderId, CancellationToken ct);
}
