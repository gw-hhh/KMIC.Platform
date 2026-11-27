using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Contracts;

namespace Kmic.Operations.Application.BackgroundJobs.Mine;

public sealed class ListMyJobsQueryHandler(IExecutionContext execution, IModuleJobStores stores, ICursorProtector protector, IUserReferences users)
    : IQueryHandler<ListMyJobsQuery, CursorPage<BackgroundJobItem>>
{
    public async ValueTask<Result<CursorPage<BackgroundJobItem>>> Handle(ListMyJobsQuery query, CancellationToken cancellationToken)
    {
        // 已登录请求必定关联用户；系统身份无归属的“我的作业”
        if (execution.User.UserId is not { } me)
        {
            return new CursorPage<BackgroundJobItem>([], null);
        }

        BackgroundJobState? state = null;
        if (query.Status is { Length: > 0 } status)
        {
            if (!Enum.TryParse<BackgroundJobState>(status, ignoreCase: false, out var parsed) || !Enum.IsDefined(parsed))
            {
                return CommonErrors.ValidationFailedWith([new FieldError("status", "Invalid", "无效的作业状态")]);
            }

            state = parsed;
        }

        return await BackgroundJobPaging.PageAsync(stores.All, new JobFilter(State: state, SubmittedBy: me.Value), query.Cursor, protector, users, cancellationToken);
    }
}
