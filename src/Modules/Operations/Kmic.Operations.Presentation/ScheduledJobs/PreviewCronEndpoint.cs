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
            .WithSummary("校验 Cron 并预览后续触发时间")
            .WithDescription("使用业务时区与含秒 Quartz Cron（如 0 0 7 * * ?）；格式无效、无后续触发或间隔不足 1 分钟返回 cron 字段错误");

    private static async Task<IResult> HandleAsync([Description("Cron 表达式")] string cron, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new PreviewCronQuery(cron), ct)).ToOk();
}
