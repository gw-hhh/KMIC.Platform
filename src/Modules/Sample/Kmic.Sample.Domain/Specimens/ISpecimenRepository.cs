using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Sample.Domain.Specimens;

/// <summary>
/// 示例标本的仓储
/// </summary>
public interface ISpecimenRepository : IRepository<Specimen, SpecimenId>;
