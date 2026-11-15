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
            .WithDescription("仅返回未作废且未到绝对到期时间的会话，包含以该用户身份创建或由其发起的模拟会话；返回设备、IP、登录及最近刷新时间。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListUserSessionsQuery(id), ct)).ToOk();
}
