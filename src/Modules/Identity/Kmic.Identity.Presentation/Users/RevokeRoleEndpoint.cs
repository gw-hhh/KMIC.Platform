using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Users.Grants.RevokeRole;

namespace Kmic.Identity.Presentation.Users;

internal sealed class RevokeRoleEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/users/{id}/grants/{roleId}/revoke", HandleAsync)
            .Sends<RevokeRoleCommand>()
            .WithSummary("收回角色")
            .WithDescription("收回后用户须重新选角；不可修改自己，收回受保护角色时操作者当前角色也须受保护，且须保留至少一位可登录的受保护角色持有人");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, [Description("角色 ID")] RoleId roleId, RevokeRoleRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RevokeRoleCommand(id, request.ExpectedVersion, roleId), ct)).ToOk();
}
