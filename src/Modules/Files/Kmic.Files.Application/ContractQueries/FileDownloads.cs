using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>
/// 通过查询管道获取文件下载信息
/// </summary>
public sealed class FileDownloads(IContractQuerySender sender) : IFileDownloads
{
    /// <inheritdoc />
    public async Task<Result<FileDownload>> GetAsync(Guid fileId, CancellationToken ct) =>
        await sender.SendAsync(new GetFileDownloadQuery(fileId), ct);
}
