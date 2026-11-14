using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Contracts;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 通过查询管道分批解析组织单元引用
/// </summary>
public sealed class OrgUnitReferences(IContractQuerySender sender) : IOrgUnitReferences
{
    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var resolved = await ContractBatch.ChunkedAsync(
            ids,
            ContractBatch.DefaultSize,
            async (batch, token) => await sender.SendAsync(new GetOrgUnitReferencesQuery([.. batch.Select(OrgUnitId.From)]), token),
            ct);
        return resolved.IsSuccess
            ? resolved.Value
            : throw new InvalidOperationException($"解析组织单元引用失败：{resolved.Error.Code}。");
    }
}
