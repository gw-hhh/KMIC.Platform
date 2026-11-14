using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Users.Grants.GrantRole;

namespace Kmic.Identity.Presentation.Users;

internal sealed class GrantRoleEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/users/{id}/grants", HandleAsync)
            .Sends<GrantRoleCommand>()
            .WithSummary("授予角色")
            .WithDescription("角色及组织锚点必须已启用。不能修改自己的角色授予；授予受保护角色时，操作者的当前角色也必须受保护。必须提供幂等键。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, GrantRoleRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GrantRoleCommand(id, request.ExpectedVersion, request.RoleId, request.Anchors ?? []), ct)).ToOk();
}
