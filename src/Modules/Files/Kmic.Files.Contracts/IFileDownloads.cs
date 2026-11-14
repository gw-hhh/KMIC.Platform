using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>
/// 按文件 ID 取下载信息
/// </summary>
/// <remarks>
/// 调用方须先校验业务对象或作业的访问权限，并确认文件归属。
/// 文件状态由本接口检查，允许下载已关联文件，以及已登记且可用的模块生成文件
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
