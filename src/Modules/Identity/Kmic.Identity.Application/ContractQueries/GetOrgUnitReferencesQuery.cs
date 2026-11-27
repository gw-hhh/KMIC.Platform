using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>批量查询组织单元引用，供 <see cref="Kmic.Identity.Contracts.IOrgUnitReferences"/> 调用</summary>
/// <param name="Ids">组织单元 ID</param>
[ContractQuery(ContractAccess.Reference)]
public sealed record GetOrgUnitReferencesQuery(IReadOnlyCollection<OrgUnitId> Ids) : IQuery<IReadOnlyDictionary<Guid, ReferenceInfo>>;
