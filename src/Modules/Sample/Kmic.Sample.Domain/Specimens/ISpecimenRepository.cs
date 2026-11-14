using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Sample.Domain.Specimens;

/// <summary>
/// 示例的仓储
/// </summary>
public interface ISpecimenRepository : IRepository<Specimen, SpecimenId>;
