using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>管理业务对象的文件关联，必须在业务命令事务内调用</summary>
public interface IFileAttachments
{
    /// <summary>关联一批文件，单次最多 50 个（可配置）</summary>
    /// <returns>成功返回文件快照；不可关联返回 Files.NotAttachable，超限返回 Files.TooManyFiles，获取锁超时返回临时冲突错误</returns>
    Task<Result<IReadOnlyList<FileSnapshot>>> AttachAsync(string ownerModule, string ownerType, Guid ownerId, IReadOnlyCollection<Guid> fileIds, CancellationToken ct);

    /// <summary>解除一批文件的关联，必须与删除业务关联记录处于同一事务内</summary>
    Task DetachAsync(string ownerModule, string ownerType, Guid ownerId, IReadOnlyCollection<Guid> fileIds, CancellationToken ct);
}
