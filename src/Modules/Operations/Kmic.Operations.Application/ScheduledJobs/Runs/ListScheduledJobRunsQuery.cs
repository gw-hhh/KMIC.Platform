using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ScheduledJobs.Runs;

/// <summary>查询单个定时任务的执行记录</summary>
/// <param name="JobName">任务名</param>
/// <param name="Cursor">游标分页参数</param>
[RequirePermission(OperationsPermissions.JobsView)]
public sealed record ListScheduledJobRunsQuery(string JobName, CursorRequest Cursor) : IQuery<CursorPage<ScheduledJobRunItem>>;
