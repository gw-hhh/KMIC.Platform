using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ScheduledJobs.Preview;

/// <summary>按业务时区校验 Cron 表达式并预览后续触发时间</summary>
/// <param name="Cron">Cron 表达式</param>
[RequirePermission(OperationsPermissions.JobsView)]
public sealed record PreviewCronQuery(string Cron) : IQuery<CronPreviewResponse>;
