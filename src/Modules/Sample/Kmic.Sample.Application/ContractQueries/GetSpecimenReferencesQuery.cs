using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.ContractQueries;

/// <summary>
/// 批量查询示例引用，供 <see cref="Kmic.Sample.Contracts.ISpecimenReferences"/> 调用
/// </summary>
/// <param name="Ids">示例 ID</param>
[ContractQuery(ContractAccess.Reference)]
public sealed record GetSpecimenReferencesQuery(IReadOnlyCollection<SpecimenId> Ids) : IQuery<IReadOnlyDictionary<Guid, ReferenceInfo>>;
