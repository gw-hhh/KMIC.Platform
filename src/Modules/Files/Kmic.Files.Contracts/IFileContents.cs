using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>
/// 读取文件内容，只给系统身份用
/// </summary>
/// <remarks>
/// 用于系统读取邮件附件；用户下载须经业务对象授权并使用 IFileDownloads。
/// 可读取已关联、可用或已解除关联的文件；上传中、扫描中返回可重试的 Files.NotReady，已彻底删除返回 Files.Purged，其余按不存在处理。
/// 存储对象缺失返回 Common.FileContentMissing
/// </remarks>
public interface IFileContents
{
    /// <summary>
    /// 打开文件内容
    /// </summary>
    /// <returns>文件内容，流由调用方释放</returns>
    Task<Result<FileContent>> OpenReadAsync(Guid fileId, CancellationToken ct);
}
