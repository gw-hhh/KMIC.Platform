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
            .WithDescription("切换至已获授且启用的角色并返回新访问令牌；令牌与会话角色不一致时拒绝，模拟会话不可切换，当前角色不可用时仍可重选");

    private static async Task<IResult> HandleAsync(SwitchRoleRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SwitchRoleCommand(request.RoleId), ct)).ToOk();
}
