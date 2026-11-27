using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.ContractQueries;

public sealed class FileAttachability(IContractQuerySender sender) : IFileAttachability
{
    public async Task<IReadOnlyList<FileSnapshot>> CheckAsync(IReadOnlyCollection<Guid> fileIds, Guid uploaderId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(fileIds);

        var result = await sender.SendAsync(new CheckAttachabilityQuery(fileIds, uploaderId), ct);
        return result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException($"查询文件关联条件失败：{result.Error.Code}。");
    }
}
