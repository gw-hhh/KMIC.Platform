using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Authentication;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Authentication.GetMe;

namespace Kmic.Identity.Presentation.Authentication;

internal sealed class GetMeEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/me", HandleAsync)
            .Sends<GetMeQuery>()
            .AllowWhenRoleUnavailable()
            .WithSummary("当前用户")
            .WithDescription("当前用户、当前角色、可切换的角色、当前角色的权限点、授权版本、是否必须改密码、是否模拟身份和业务时区。当前角色不可用时也能调用，前端据此引导重新选择角色。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetMeQuery(), ct)).ToOk();
}
