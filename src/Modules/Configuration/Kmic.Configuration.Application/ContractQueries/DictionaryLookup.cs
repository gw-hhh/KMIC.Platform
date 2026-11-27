using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.Configuration.Contracts;

namespace Kmic.Configuration.Application.ContractQueries;

public sealed class DictionaryLookup(IContractQuerySender sender) : IDictionaries
{
    public async Task<DictionarySnapshot?> GetAsync(string typeCode, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeCode);

        var result = await sender.SendAsync(new GetDictionarySnapshotQuery(typeCode), ct);
        return result.IsSuccess ? result.Value : throw new InvalidOperationException($"读取字典 {typeCode} 失败：{result.Error.Code}。");
    }

    public async Task<bool> IsSelectableAsync(string typeCode, string itemCode, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemCode);

        var snapshot = await GetAsync(typeCode, ct);
        return snapshot?.Items.Any(i => i.IsActive && string.Equals(i.Code, itemCode, StringComparison.OrdinalIgnoreCase)) == true;
    }
}
