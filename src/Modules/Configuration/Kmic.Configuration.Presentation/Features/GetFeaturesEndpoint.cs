using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Features;

namespace Kmic.Configuration.Presentation.Features;

internal sealed class GetFeaturesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/features", HandleAsync)
            .Sends<GetFeaturesQuery>()
            .WithSummary("查询当前开放的功能开关")
            .WithDescription("登录后可查询已开启的功能开关，供前端控制入口显示。开关来自部署配置，只决定功能是否开放，不授予权限。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetFeaturesQuery(), ct)).ToOk();
}
