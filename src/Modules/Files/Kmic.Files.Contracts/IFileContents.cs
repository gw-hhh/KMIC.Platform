using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Files.Contracts;

/// <summary>读取文件内容，仅限系统身份调用；用户下载请使用 <see cref="IFileDownloads" /></summary>
public interface IFileContents
{
    /// <summary>打开文件内容，返回的流由调用方释放</summary>
    Task<Result<FileContent>> OpenReadAsync(Guid fileId, CancellationToken ct);
}
