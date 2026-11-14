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
            .WithDescription("登录就能看。返回开启的开关名，前端启动时拉一次，据此显示或隐藏入口；开关只决定功能是否开放，不授予权限。开关的值来自部署配置，改了要重启生效。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetFeaturesQuery(), ct)).ToOk();
}
