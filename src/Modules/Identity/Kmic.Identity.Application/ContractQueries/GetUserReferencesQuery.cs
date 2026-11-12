using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 按 ID 批量解析用户的引用信息，只给其他模块通过 <see cref="Kmic.Identity.Contracts.IUserReferences"/> 调用
/// </summary>
/// <param name="Ids">用户 ID，一次最多 1000 个</param>
[ContractQuery(ContractAccess.Reference)]
public sealed record GetUserReferencesQuery(IReadOnlyCollection<UserId> Ids) : IQuery<IReadOnlyDictionary<Guid, ReferenceInfo>>;
