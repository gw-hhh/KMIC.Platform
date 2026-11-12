using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Contracts;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// <see cref="IUserContacts"/> 的实现：薄适配器，只把调用转成本模块的查询，经管道执行
/// </summary>
public sealed class UserContacts(IContractQuerySender sender) : IUserContacts
{
    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, UserContact>> GetAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(userIds);

        var resolved = await ContractBatch.ChunkedAsync(
            userIds,
            ContractBatch.DefaultSize,
            async (batch, token) => await sender.SendAsync(new GetUserContactsQuery([.. batch.Select(UserId.From)]), token),
            ct);
        return resolved.IsSuccess
            ? resolved.Value
            : throw new InvalidOperationException($"取用户联系方式失败：{resolved.Error.Code}。");
    }
}
