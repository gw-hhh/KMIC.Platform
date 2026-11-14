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
            .NoStore()
            .AllowWhenRoleUnavailable()
            .WithSummary("切换当前角色")
            .WithDescription("切换到用户已获授且启用的角色，返回新访问令牌。令牌角色与会话当前角色不一致时，请求会被拒绝。当前角色被收回或停用后仍可重新选择；模拟会话不能切换角色。");

    private static async Task<IResult> HandleAsync(SwitchRoleRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SwitchRoleCommand(request.RoleId), ct)).ToOk();
}
