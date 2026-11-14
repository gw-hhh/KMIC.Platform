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
            .WithDescription("返回本人的有效登录会话，包括设备、IP、登录时间、最近刷新时间和当前会话标记，不包含模拟会话。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListMySessionsQuery(), ct)).ToOk();
}
