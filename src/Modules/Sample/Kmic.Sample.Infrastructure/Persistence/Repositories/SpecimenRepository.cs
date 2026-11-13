using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Sample.Infrastructure.Persistence.Repositories;

internal sealed class SpecimenRepository(SampleDbContext context)
    : EfRepository<SampleDbContext, Specimen, SpecimenId>(context), ISpecimenRepository
{
    protected override Error NotFound => SampleErrors.NotFound;

    // 聚合的明细和附件总是一起加载
    protected override IQueryable<Specimen> Aggregate(IQueryable<Specimen> set) => set.Include(s => s.Items).Include(s => s.Attachments);
}
