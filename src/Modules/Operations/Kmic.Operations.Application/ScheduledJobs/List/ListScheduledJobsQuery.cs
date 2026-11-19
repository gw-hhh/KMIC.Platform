using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ScheduledJobs.List;

/// <summary>
/// 查询全部定时任务
/// </summary>
/// <remarks>
/// 包括模块和框架的任务，按模块、名称排序；已移除声明但还有自定义设置的任务也会返回
/// </remarks>
[RequirePermission(OperationsPermissions.JobsView)]
public sealed record ListScheduledJobsQuery : IQuery<IReadOnlyList<ScheduledJobItem>>;
