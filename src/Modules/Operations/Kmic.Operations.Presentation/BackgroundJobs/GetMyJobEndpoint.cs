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
            .WithSummary("查询我提交的一个后台作业")
            .WithDescription("页面轮询进度用，登录就能看，建议每 2 秒查一次、结束后停止。别人提交的作业和不存在的一样返回 404 Operations.BackgroundJobNotFound。");

    private static async Task<IResult> HandleAsync(
        [Description("作业所在的模块，如 Sample")] string module,
        [Description("作业 ID")] Guid id,
        IQuerySender sender,
        CancellationToken ct) =>
        (await sender.SendAsync(new GetMyJobQuery(module, id), ct)).ToOk();
}
