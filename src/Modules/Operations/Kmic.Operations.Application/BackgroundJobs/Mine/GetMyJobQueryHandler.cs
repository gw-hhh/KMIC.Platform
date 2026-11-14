using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Contracts;
using Kmic.Operations.Domain.Errors;

namespace Kmic.Operations.Application.BackgroundJobs.Mine;

/// <summary>
/// 处理 <see cref="GetMyJobQuery"/>
/// </summary>
public sealed class GetMyJobQueryHandler(IExecutionContext execution, IModuleJobStores stores, IUserReferences users)
    : IQueryHandler<GetMyJobQuery, BackgroundJobItem>
{
    /// <inheritdoc />
    public async ValueTask<Result<BackgroundJobItem>> Handle(GetMyJobQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var job = await MyJobs.FindAsync(execution, stores, query.Module, query.JobId, cancellationToken);
        if (job is null)
        {
            return OperationsErrors.BackgroundJobNotFound;
        }

        var names = await users.ResolveAsync([.. new[] { job.SubmittedBy, job.CancelRequestedBy }.OfType<Guid>().Distinct()], cancellationToken);
        return BackgroundJobPaging.ToItem(job, names);
    }
}
