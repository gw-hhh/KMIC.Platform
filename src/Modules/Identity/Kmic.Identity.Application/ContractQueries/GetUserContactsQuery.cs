using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Identity.Contracts;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 批量查询用户联系方式
/// </summary>
/// <param name="Ids">用户 ID，一次最多 1000 个</param>
[ContractQuery(ContractAccess.SystemOnly)]
public sealed record GetUserContactsQuery(IReadOnlyCollection<UserId> Ids) : IQuery<IReadOnlyDictionary<Guid, UserContact>>;
