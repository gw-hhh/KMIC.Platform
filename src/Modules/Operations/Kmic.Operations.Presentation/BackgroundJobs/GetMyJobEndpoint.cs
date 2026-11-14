using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.BackgroundJobs.Mine;

namespace Kmic.Operations.Presentation.BackgroundJobs;

internal sealed class GetMyJobEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/my-jobs/{module}/{id}", HandleAsync)
            .Sends<GetMyJobQuery>()
            .WithSummary("查询本人提交的后台作业详情")
            .WithDescription("登录后可查询当前用户提交的作业。建议每 2 秒轮询一次，作业结束后停止。作业不存在或由其他用户提交时，均返回 404 Operations.BackgroundJobNotFound。");

    private static async Task<IResult> HandleAsync(
        [Description("作业所在的模块，如 Sample")] string module,
        [Description("作业 ID")] Guid id,
        IQuerySender sender,
        CancellationToken ct) =>
        (await sender.SendAsync(new GetMyJobQuery(module, id), ct)).ToOk();
}
