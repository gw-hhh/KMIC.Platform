using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ScheduledJobs.Runs;

/// <summary>
/// 查询一个定时任务的执行记录
/// </summary>
/// <remarks>
/// 按开始时间倒序游标分页；已移除声明的任务也能查历史记录
/// </remarks>
/// <param name="JobName">任务名</param>
/// <param name="Cursor">游标分页参数</param>
[RequirePermission(OperationsPermissions.JobsView)]
public sealed record ListScheduledJobRunsQuery(string JobName, CursorRequest Cursor) : IQuery<CursorPage<ScheduledJobRunItem>>;
