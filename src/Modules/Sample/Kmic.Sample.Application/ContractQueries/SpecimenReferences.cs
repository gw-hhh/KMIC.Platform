using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.Sample.Contracts;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.ContractQueries;

/// <summary>
/// 为其他模块提供示例引用查询
/// </summary>
/// <remarks>
/// 通过 <see cref="ContractBatch"/> 分批查询引用信息。
/// 查询失败时抛出异常
/// </remarks>
public sealed class SpecimenReferences(IContractQuerySender sender) : ISpecimenReferences
{
    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var resolved = await ContractBatch.ChunkedAsync(
            ids,
            ContractBatch.DefaultSize,
            async (batch, token) => await sender.SendAsync(new GetSpecimenReferencesQuery([.. batch.Select(SpecimenId.From)]), token),
            ct);
        return resolved.IsSuccess
            ? resolved.Value
            : throw new InvalidOperationException($"解析示例引用失败：{resolved.Error.Code}。");
    }
}
