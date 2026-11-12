using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.Sample.Contracts;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.ContractQueries;

/// <summary>
/// <see cref="ISpecimenReferences"/> 的实现：薄适配器，只把调用转成本模块的查询，经管道执行
/// </summary>
/// <remarks>
/// 批量与分批由 <see cref="ContractBatch"/> 处理。引用解析没有业务上的失败，失败只可能是数据库不可用这类基础设施问题，
/// 所以失败时抛异常，不让每个调用方都去处理 Result
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
            : throw new InvalidOperationException($"解析标本引用失败：{resolved.Error.Code}。");
    }
}
