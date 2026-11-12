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
            .WithDescription("角色必须启用，锚点必须是启用的组织单元。不能改自己的授予；受保护角色只有当前角色也是受保护角色的人才能授予。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, GrantRoleRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GrantRoleCommand(id, request.ExpectedVersion, request.RoleId, request.Anchors ?? []), ct)).ToOk();
}
