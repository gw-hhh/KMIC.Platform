using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Contracts;

namespace Kmic.Operations.Application.BackgroundJobs.Mine;

/// <summary>
/// 处理 <see cref="ListMyJobsQuery"/>
/// </summary>
public sealed class ListMyJobsQueryHandler(IExecutionContext execution, IModuleJobStores stores, ICursorProtector protector, IUserReferences users)
    : IQueryHandler<ListMyJobsQuery, CursorPage<BackgroundJobItem>>
{
    /// <inheritdoc />
    public async ValueTask<Result<CursorPage<BackgroundJobItem>>> Handle(ListMyJobsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        // 登录的请求一定有用户；没有时（系统身份）没有“我的作业”
        if (execution.User.UserId is not { } me)
        {
            return new CursorPage<BackgroundJobItem>([], null);
        }

        BackgroundJobState? state = null;
        if (query.Status is { Length: > 0 } status)
        {
            if (!Enum.TryParse<BackgroundJobState>(status, ignoreCase: false, out var parsed) || !Enum.IsDefined(parsed))
            {
                return CommonErrors.ValidationFailedWith([new FieldError("status", "Invalid", "状态不认识")]);
            }

            state = parsed;
        }

        return await BackgroundJobPaging.PageAsync(stores.All, new JobFilter(State: state, SubmittedBy: me.Value), query.Cursor, protector, users, cancellationToken);
    }
}
