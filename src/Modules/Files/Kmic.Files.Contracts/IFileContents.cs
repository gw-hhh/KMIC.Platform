using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>
/// 读取文件内容，只给系统身份用
/// </summary>
/// <remarks>
/// 用户下载走 IFileDownloads
/// </remarks>
public interface IFileContents
{
    /// <summary>
    /// 打开文件内容
    /// </summary>
    /// <returns>文件内容，流由调用方释放</returns>
    Task<Result<FileContent>> OpenReadAsync(Guid fileId, CancellationToken ct);
}
