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
            .WithDescription("仅返回本人未作废且未到绝对到期时间的登录会话，不含模拟会话。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListMySessionsQuery(), ct)).ToOk();
}
