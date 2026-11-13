using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>
/// 业务对象关联、解除关联文件的唯一入口
/// </summary>
/// <remarks>
/// 只能在命令里用。关联与 Files 的清理按文件 ID 的数据库锁互斥，不靠时钟：先取共享咨询锁，再检查文件现在能不能关联，
/// 最后发 <see cref="AttachFiles" />，锁持有到业务事务提交。业务代码不要直接发 AttachFiles，
/// 重新关联、把生成的文件关联到业务对象也都走这里
/// </remarks>
public interface IFileAttachments
{
    /// <summary>
    /// 关联一批文件
    /// </summary>
    /// <param name="ownerModule">业务模块名</param>
    /// <param name="ownerType">业务对象的类型，如 Specimen</param>
    /// <param name="ownerId">业务对象的 ID</param>
    /// <param name="fileIds">要关联的文件，一次最多 50 个</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>写业务关联行需要的文件快照；有文件不能关联时返回 Files.NotAttachable，文件太多返回 Files.TooManyFiles，等锁太久返回暂时冲突</returns>
    Task<Result<IReadOnlyList<FileSnapshot>>> AttachAsync(string ownerModule, string ownerType, Guid ownerId, IReadOnlyCollection<Guid> fileIds, CancellationToken ct);

    /// <summary>
    /// 解除关联：和删除业务关联行放在同一个业务事务里
    /// </summary>
    /// <param name="ownerModule">业务模块名</param>
    /// <param name="ownerType">业务对象的类型，如 Specimen</param>
    /// <param name="ownerId">业务对象的 ID</param>
    /// <param name="fileIds">要解除关联的文件</param>
    /// <param name="ct">取消令牌</param>
    Task DetachAsync(string ownerModule, string ownerType, Guid ownerId, IReadOnlyCollection<Guid> fileIds, CancellationToken ct);
}
