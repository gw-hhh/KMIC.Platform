using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.ScheduledJobs.Preview;

namespace Kmic.Operations.Presentation.ScheduledJobs;

internal sealed class PreviewCronEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/scheduled-jobs/cron-preview", HandleAsync)
            .Sends<PreviewCronQuery>()
            .WithSummary("检查 Cron 表达式并预览接下来 5 次触发时间")
            .WithDescription("按业务时区解释，写法是 Quartz 的（带秒，如 0 0 7 * * ?）。写法不对、以后不会再触发、两次执行间隔短于 1 分钟，都返回 cron 字段的错误。");

    private static async Task<IResult> HandleAsync([Description("Cron 表达式")] string cron, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new PreviewCronQuery(cron), ct)).ToOk();
}
