using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>
/// 读取文件内容，只给系统身份用
/// </summary>
/// <remarks>
/// 给 Notifications 投递邮件时读取附件。下载走业务对象的授权和 <see cref="IFileDownloads" />，不经这里。
/// 已关联、可用（还没被关联）、已解除关联的文件可以读；已经彻底删除的返回 410 Files.Purged，还没登记完的返回可重试的
/// Files.NotReady，其余按不存在处理。对象在存储里读不到返回 Common.FileContentMissing
/// </remarks>
public interface IFileContents
{
    /// <summary>
    /// 打开文件内容
    /// </summary>
    /// <param name="fileId">文件 ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>文件内容，流由调用方释放</returns>
    Task<Result<FileContent>> OpenReadAsync(Guid fileId, CancellationToken ct);
}
