using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Authentication;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Authentication.SwitchRole;

namespace Kmic.Identity.Presentation.Authentication;

internal sealed class SwitchRoleEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/auth/switch-role", HandleAsync)
            .Sends<SwitchRoleCommand>()
            .AllowWhenRoleUnavailable()
            .WithSummary("切换当前角色")
            .WithDescription("在同一会话里换成用户拥有的另一个启用的角色，返回带新角色的访问令牌；旧角色的访问令牌随即失效。当前角色被收回或停用后也能用它重新选择角色。");

    private static async Task<IResult> HandleAsync(SwitchRoleRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SwitchRoleCommand(request.RoleId), ct)).ToOk();
}
