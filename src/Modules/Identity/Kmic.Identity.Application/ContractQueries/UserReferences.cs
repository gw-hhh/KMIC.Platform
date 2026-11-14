using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Contracts;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 通过查询管道分批解析用户引用
/// </summary>
/// <remarks>
/// 由 <see cref="ContractBatch"/> 分批查询；管道返回失败时抛异常，由统一异常处理机制接管
/// </remarks>
public sealed class UserReferences(IContractQuerySender sender) : IUserReferences
{
    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, ReferenceInfo>> ResolveAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(ids);

        var resolved = await ContractBatch.ChunkedAsync(
            ids,
            ContractBatch.DefaultSize,
            async (batch, token) => await sender.SendAsync(new GetUserReferencesQuery([.. batch.Select(UserId.From)]), token),
            ct);
        return resolved.IsSuccess
            ? resolved.Value
            : throw new InvalidOperationException($"解析用户引用失败：{resolved.Error.Code}。");
    }
}
