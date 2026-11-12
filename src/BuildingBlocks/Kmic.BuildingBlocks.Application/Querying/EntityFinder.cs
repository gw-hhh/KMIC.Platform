using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Security;
using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace Kmic.BuildingBlocks.Application.Querying;

internal sealed class EntityFinder(IExecutionContext execution, ISecurityEventWriter securityEvents) : IEntityFinder
{
    public async Task<Result<TEntity>> FindByIdOrNotFoundAsync<TEntity, TId>(
        IQueryable<TEntity> query, TId id, Error notFound, CancellationToken cancellationToken)
        where TEntity : Entity<TId>
        where TId : struct, IStronglyTypedId
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(notFound);

        var found = await query.SingleOrDefaultAsync(EntityPredicates.HasId<TEntity, TId>(id), cancellationToken);
        if (found is not null)
        {
            return found;
        }

        // 软删除的过滤仍然生效：已删除的对象不算“被数据范围挡住”
        if (await query.WithoutDataScope().AnyAsync(EntityPredicates.HasId<TEntity, TId>(id), cancellationToken))
        {
            securityEvents.Write(new SecurityEvent("AuthorizationDenied", SecuritySeverity.Warning, nameof(DenialReason.OutOfDataScope))
            {
                UserId = execution.User.UserId?.Value,
                ResourceType = typeof(TEntity).Name,
                ResourceId = id.Value,
            });
        }

        return notFound;
    }
}
