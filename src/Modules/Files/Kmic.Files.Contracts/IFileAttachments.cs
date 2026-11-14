using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>
/// 为业务对象关联文件或解除文件关联
/// </summary>
/// <remarks>
/// 必须在业务命令的事务中调用，关联和解除关联均通过本接口完成。
/// 关联时先按文件 ID 获取共享咨询锁，再检查文件是否允许关联，最后发送 <see cref="AttachFiles" />。
/// 文件锁持有到业务事务结束，防止清理任务在关联期间删除文件。
/// 重新关联、关联系统生成的文件也必须通过本接口，业务代码不得直接发送 AttachFiles
/// </remarks>
public interface IFileAttachments
{
    /// <summary>
    /// 关联一批文件
    /// </summary>
    /// <param name="ownerModule">业务模块名</param>
    /// <param name="ownerType">业务对象的类型，如 Specimen</param>
    /// <param name="ownerId">业务对象的 ID</param>
    /// <param name="fileIds">要关联的文件 ID，去重后的数量上限由服务端配置，默认 50 个</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>
    /// 成功时返回用于保存业务关联的文件快照。
    /// 文件不可关联时返回 Files.NotAttachable，超过数量上限时返回 Files.TooManyFiles。
    /// 获取文件锁超时时返回暂时冲突错误
    /// </returns>
    Task<Result<IReadOnlyList<FileSnapshot>>> AttachAsync(string ownerModule, string ownerType, Guid ownerId, IReadOnlyCollection<Guid> fileIds, CancellationToken ct);

    /// <summary>
    /// 解除一批文件的关联
    /// </summary>
    /// <remarks>
    /// 必须与删除业务关联记录在同一个业务事务中完成
    /// </remarks>
    /// <param name="ownerModule">业务模块名</param>
    /// <param name="ownerType">业务对象的类型，如 Specimen</param>
    /// <param name="ownerId">业务对象的 ID</param>
    /// <param name="fileIds">要解除关联的文件 ID</param>
    /// <param name="ct">取消令牌</param>
    Task DetachAsync(string ownerModule, string ownerType, Guid ownerId, IReadOnlyCollection<Guid> fileIds, CancellationToken ct);
}
