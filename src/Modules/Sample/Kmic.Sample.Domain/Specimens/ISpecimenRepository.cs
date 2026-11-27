using Kmic.BuildingBlocks.Domain.Repositories;

namespace Kmic.Sample.Domain.Specimens;

public interface ISpecimenRepository : IRepository<Specimen, SpecimenId>;
