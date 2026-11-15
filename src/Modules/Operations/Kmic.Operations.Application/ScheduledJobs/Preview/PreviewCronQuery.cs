using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ScheduledJobs.Preview;

/// <summary>
/// 校验 Cron 并预览后续触发时间
/// </summary>
/// <remarks>
/// 按业务时区解释；语法无效、无后续触发时间，或预览中的触发间隔短于 1 分钟时返回字段错误
/// </remarks>
/// <param name="Cron">Cron 表达式，Quartz 写法，带秒，如 0 0 7 * * ?</param>
[RequirePermission(OperationsPermissions.JobsView)]
public sealed record PreviewCronQuery(string Cron) : IQuery<CronPreviewResponse>;
