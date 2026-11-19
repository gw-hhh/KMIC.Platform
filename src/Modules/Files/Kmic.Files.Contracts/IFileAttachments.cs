using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>
/// 为业务对象关联文件或解除文件关联
/// </summary>
/// <remarks>
/// 必须在业务命令的事务里调用
/// </remarks>
public interface IFileAttachments
{
    /// <summary>
    /// 关联一批文件
    /// </summary>
    /// <remarks>
    /// 最多 50 个文件（可配置）
    /// </remarks>
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
    /// 要和删除业务关联记录在同一事务里
    /// </remarks>
    Task DetachAsync(string ownerModule, string ownerType, Guid ownerId, IReadOnlyCollection<Guid> fileIds, CancellationToken ct);
}
