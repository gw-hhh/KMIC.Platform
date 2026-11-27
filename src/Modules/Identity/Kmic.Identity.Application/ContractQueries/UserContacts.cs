using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Contracts;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>通过查询管道分批解析用户联系方式</summary>
public sealed class UserContacts(IContractQuerySender sender) : IUserContacts
{
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
            : throw new InvalidOperationException($"获取用户联系方式失败：{resolved.Error.Code}。");
    }
}
