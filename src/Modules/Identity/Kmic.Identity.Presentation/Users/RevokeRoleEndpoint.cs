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
            .WithDescription("正在用这个角色的会话，下一个请求起得到 403，要重新选择角色。不能改自己的授予；受保护角色只有当前角色也是受保护角色的人才能收回，也不能收回最后一个启用的系统管理员的角色。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, [Description("角色 ID")] RoleId roleId, RevokeRoleRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RevokeRoleCommand(id, request.ExpectedVersion, roleId), ct)).ToOk();
}
