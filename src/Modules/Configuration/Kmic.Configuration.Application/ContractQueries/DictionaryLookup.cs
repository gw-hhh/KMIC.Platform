using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.ContractQueries;

/// <summary>
/// 通过查询管道读取字典快照并检查字典项是否可选
/// </summary>
/// <remarks>
/// 类型不存在时返回 null；查询管道返回失败时抛异常，由统一异常处理机制接管
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
