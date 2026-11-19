using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ScheduledJobs.Preview;

/// <summary>
/// 校验 Cron 并预览后续触发时间
/// </summary>
/// <remarks>
/// 按业务时区解释
/// </remarks>
/// <param name="Cron">Cron 表达式</param>
[RequirePermission(OperationsPermissions.JobsView)]
public sealed record PreviewCronQuery(string Cron) : IQuery<CronPreviewResponse>;
