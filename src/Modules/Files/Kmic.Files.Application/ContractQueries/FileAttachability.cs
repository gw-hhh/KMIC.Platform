using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.ContractQueries;

/// <summary>
/// <see cref="IFileAttachability"/> 的实现：薄适配器，只把调用转成本模块的查询，经管道执行
/// </summary>
public sealed class FileAttachability(IContractQuerySender sender) : IFileAttachability
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<FileSnapshot>> CheckAsync(IReadOnlyCollection<Guid> fileIds, Guid uploaderId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(fileIds);

        var result = await sender.SendAsync(new CheckAttachabilityQuery(fileIds, uploaderId), ct);
        return result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException($"检查文件能不能关联失败：{result.Error.Code}。");
    }
}
