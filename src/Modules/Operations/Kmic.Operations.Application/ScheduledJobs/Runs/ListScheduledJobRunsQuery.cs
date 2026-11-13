using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ScheduledJobs.Runs;

/// <summary>
/// 查询一个定时任务的执行记录
/// </summary>
/// <remarks>
/// 按开始时间倒序，游标分页。已经从代码里删除的任务，留下的记录也能查
/// </remarks>
/// <param name="JobName">任务名</param>
/// <param name="Cursor">游标分页参数</param>
[RequirePermission(OperationsPermissions.JobsView)]
public sealed record ListScheduledJobRunsQuery(string JobName, CursorRequest Cursor) : IQuery<CursorPage<ScheduledJobRunItem>>;
