using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 通用仓储，由 <c>AddRepository</c> 注册
/// </summary>
internal sealed class ModuleRepository<TContext, TAggregate, TId>(
    TContext context,
    Error notFound,
    Func<IQueryable<TAggregate>, IQueryable<TAggregate>>? aggregate)
    : EfRepository<TContext, TAggregate, TId>(context)
    where TContext : ModuleDbContext
    where TAggregate : AggregateRoot<TId>
    where TId : struct, IStronglyTypedId
{
    protected override Error NotFound => notFound;

    protected override IQueryable<TAggregate> Aggregate(IQueryable<TAggregate> set) => aggregate is null ? set : aggregate(set);
}
