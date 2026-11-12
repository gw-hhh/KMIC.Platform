using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Roles.SetPermissions;

namespace Kmic.Identity.Presentation.Roles;

internal sealed class SetRolePermissionsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/roles/{id}/permissions", HandleAsync)
            .Sends<SetRolePermissionsCommand>()
            .WithSummary("整体替换角色的权限点")
            .WithDescription("只接受权限点目录里存在的编码。内置的系统管理员角色拥有全部权限点，不能手工修改。");

    private static async Task<IResult> HandleAsync([Description("角色 ID")] RoleId id, SetRolePermissionsRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetRolePermissionsCommand(id, request.ExpectedVersion, request.Permissions), ct)).ToOk();
}
