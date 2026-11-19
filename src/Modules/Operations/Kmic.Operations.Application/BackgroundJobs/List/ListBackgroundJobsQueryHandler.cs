using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Contracts;

namespace Kmic.Operations.Application.BackgroundJobs.List;

/// <summary>
/// 汇总各模块的后台作业
/// </summary>
public sealed class ListBackgroundJobsQueryHandler(IModuleJobStores stores, ICursorProtector protector, IUserReferences users)
    : IQueryHandler<ListBackgroundJobsQuery, CursorPage<BackgroundJobItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<CursorPage<BackgroundJobItem>>> Handle(ListBackgroundJobsQuery query, CancellationToken cancellationToken)
    {
        BackgroundJobState? state = null;
        if (query.Status is { Length: > 0 } status)
        {
            if (!Enum.TryParse<BackgroundJobState>(status, ignoreCase: false, out var parsed) || !Enum.IsDefined(parsed))
            {
                return CommonErrors.ValidationFailedWith([new FieldError("status", "Invalid", "无效的作业状态")]);
            }

            state = parsed;
        }

        IEnumerable<IModuleJobStore> selected = stores.All;
        if (query.Module is { Length: > 0 } module)
        {
            selected = stores.Find(module) is { } one ? [one] : [];
        }

        return await BackgroundJobPaging.PageAsync(
            selected, new JobFilter(query.JobType, state, query.SubmittedBy), query.Cursor, protector, users, cancellationToken);
    }
}
