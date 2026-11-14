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
            .WithDescription("该用户从下一次请求起不能再使用此角色，须重新选择角色。不能修改自己的角色授予；收回受保护角色时，操作者的当前角色也必须受保护。须保留至少一位可登录且持有已启用受保护角色的用户。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, [Description("角色 ID")] RoleId roleId, RevokeRoleRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RevokeRoleCommand(id, request.ExpectedVersion, roleId), ct)).ToOk();
}
