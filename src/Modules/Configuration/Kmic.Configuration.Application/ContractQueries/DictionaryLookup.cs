using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.ContractQueries;

/// <summary>
/// <see cref="IDictionaries"/> 的实现：薄适配器，只把调用转成本模块的查询，经管道执行
/// </summary>
/// <remarks>
/// 读字典没有业务上的失败，失败只可能是基础设施问题，所以失败时抛异常，不让每个调用方都去处理 Result
/// </remarks>
public sealed class DictionaryLookup(IContractQuerySender sender) : IDictionaries
{
    /// <inheritdoc />
    public async Task<DictionarySnapshot?> GetAsync(string typeCode, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeCode);

        var result = await sender.SendAsync(new GetDictionarySnapshotQuery(typeCode), ct);
        return result.IsSuccess ? result.Value : throw new InvalidOperationException($"读取字典 {typeCode} 失败：{result.Error.Code}。");
    }

    /// <inheritdoc />
    public async Task<bool> IsSelectableAsync(string typeCode, string itemCode, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemCode);

        var snapshot = await GetAsync(typeCode, ct);
        return snapshot?.Items.Any(i => i.IsActive && string.Equals(i.Code, itemCode, StringComparison.OrdinalIgnoreCase)) == true;
    }
}
