using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 批量查询用户引用，供 <see cref="Kmic.Identity.Contracts.IUserReferences"/> 调用
/// </summary>
/// <param name="Ids">用户 ID</param>
[ContractQuery(ContractAccess.Reference)]
public sealed record GetUserReferencesQuery(IReadOnlyCollection<UserId> Ids) : IQuery<IReadOnlyDictionary<Guid, ReferenceInfo>>;
