using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>按文件 ID 获取下载信息，调用方须先校验访问权限与文件归属</summary>
public interface IFileDownloads
{
    /// <summary>获取指定文件的下载信息</summary>
    /// <returns>文件下载信息；已彻底删除返回 Files.Purged（HTTP 410），尚未就绪返回可重试的 Files.NotReady，其余不可下载状态按不存在处理</returns>
    Task<Result<FileDownload>> GetAsync(Guid fileId, CancellationToken ct);
}
