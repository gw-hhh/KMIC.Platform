using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ScheduledJobs.Preview;

/// <summary>
/// 检查一个 Cron 表达式并给出接下来 5 次触发的时间
/// </summary>
/// <remarks>
/// 按业务时区解释。写法不对、以后不会再触发、两次执行间隔短于 1 分钟，都返回逐字段的错误
/// </remarks>
/// <param name="Cron">Cron 表达式，Quartz 写法，带秒，如 0 0 7 * * ?</param>
[RequirePermission(OperationsPermissions.JobsView)]
public sealed record PreviewCronQuery(string Cron) : IQuery<CronPreviewResponse>;
