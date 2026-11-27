using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Users.Sessions;

namespace Kmic.Identity.Presentation.Users;

internal sealed class ListUserSessionsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/users/{id}/sessions", HandleAsync)
            .Sends<ListUserSessionsQuery>()
            .WithSummary("查询用户有效会话")
            .WithDescription("仅返回未作废且未过期的会话（含相关模拟会话）及设备、IP 与刷新时间");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListUserSessionsQuery(id), ct)).ToOk();
}
