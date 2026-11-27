using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>批量查询角色引用，供 <see cref="Kmic.Identity.Contracts.IRoleReferences"/> 调用</summary>
/// <param name="Ids">角色 ID</param>
[ContractQuery(ContractAccess.Reference)]
public sealed record GetRoleReferencesQuery(IReadOnlyCollection<RoleId> Ids) : IQuery<IReadOnlyDictionary<Guid, ReferenceInfo>>;
