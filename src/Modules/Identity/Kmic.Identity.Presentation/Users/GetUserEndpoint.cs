using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Users.Get;

namespace Kmic.Identity.Presentation.Users;

internal sealed class GetUserEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/users/{id}", HandleAsync)
            .Sends<GetUserQuery>()
            .WithSummary("查询用户详情")
            .WithDescription("资料、角色授予与锚点、登录被限制到什么时候、有效会话数、版本号和当前用户可以对他执行的动作。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetUserQuery(id), ct)).ToOk();
}
