using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 按 ID 批量解析组织单元的引用信息，只给其他模块通过 <see cref="Kmic.Identity.Contracts.IOrgUnitReferences"/> 调用
/// </summary>
/// <param name="Ids">组织单元 ID，一次最多 1000 个</param>
[ContractQuery(ContractAccess.Reference)]
public sealed record GetOrgUnitReferencesQuery(IReadOnlyCollection<OrgUnitId> Ids) : IQuery<IReadOnlyDictionary<Guid, ReferenceInfo>>;
