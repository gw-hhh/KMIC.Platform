using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.ContractQueries;

/// <summary>
/// 按 ID 批量解析示例标本的引用信息，只给其他模块通过 <see cref="Kmic.Sample.Contracts.ISpecimenReferences"/> 调用
/// </summary>
/// <param name="Ids">标本 ID，一次最多 1000 个</param>
[ContractQuery(ContractAccess.Reference)]
public sealed record GetSpecimenReferencesQuery(IReadOnlyCollection<SpecimenId> Ids) : IQuery<IReadOnlyDictionary<Guid, ReferenceInfo>>;
