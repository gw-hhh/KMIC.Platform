using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Authentication.MySessions;

namespace Kmic.Identity.Presentation.Authentication;

internal sealed class ListMySessionsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/me/sessions", HandleAsync)
            .Sends<ListMySessionsQuery>()
            .WithSummary("查询我的登录设备")
            .WithDescription("自己当前有效的会话：设备、IP、登录时间、最近刷新时间，标出哪个是当前会话。不含别人以我的身份开始的模拟会话。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListMySessionsQuery(), ct)).ToOk();
}
