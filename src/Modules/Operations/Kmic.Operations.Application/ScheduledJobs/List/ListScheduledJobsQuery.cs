using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ScheduledJobs.List;

/// <summary>查询全部定时任务</summary>
[RequirePermission(OperationsPermissions.JobsView)]
public sealed record ListScheduledJobsQuery : IQuery<IReadOnlyList<ScheduledJobItem>>;
