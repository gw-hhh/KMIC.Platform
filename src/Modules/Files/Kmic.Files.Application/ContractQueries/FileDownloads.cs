using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.Files;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>
/// <see cref="IFileDownloads"/> 的实现：薄适配器，只把调用转成本模块的查询，经管道执行
/// </summary>
public sealed class FileDownloads(IContractQuerySender sender) : IFileDownloads
{
    /// <inheritdoc />
    public async Task<Result<FileDownload>> GetAsync(Guid fileId, CancellationToken ct) =>
        await sender.SendAsync(new GetFileDownloadQuery(fileId), ct);
}
