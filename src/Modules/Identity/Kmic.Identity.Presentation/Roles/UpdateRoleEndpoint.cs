using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Roles.Update;

namespace Kmic.Identity.Presentation.Roles;

internal sealed class UpdateRoleEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/roles/{id}", HandleAsync)
            .Sends<UpdateRoleCommand>()
            .WithSummary("修改角色的名称、说明和次序")
            .WithDescription("受保护角色只有当前角色也是受保护角色的人才能修改。");

    private static async Task<IResult> HandleAsync([Description("角色 ID")] RoleId id, UpdateRoleRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new UpdateRoleCommand(id, request.ExpectedVersion, request.Name, request.Description, request.SortOrder), ct)).ToOk();
}
