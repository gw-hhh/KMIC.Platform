using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>
/// 按文件 ID 取下载信息
/// </summary>
/// <remarks>
/// 调用方要先校验访问权限和文件归属
/// </remarks>
public interface IFileDownloads
{
    /// <summary>
    /// 取一个文件的下载信息
    /// </summary>
    /// <returns>
    /// 文件下载信息。已彻底删除时返回 Files.Purged，对应 HTTP 410。
    /// 尚未就绪时返回可重试的 Files.NotReady，其余不可下载的状态按不存在处理
    /// </returns>
    Task<Result<FileDownload>> GetAsync(Guid fileId, CancellationToken ct);
}
