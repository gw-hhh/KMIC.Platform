using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Roles.Disable;

namespace Kmic.Identity.Presentation.Roles;

internal sealed class DisableRoleEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/roles/{id}/disable", HandleAsync)
            .Sends<DisableRoleCommand>()
            .WithSummary("停用角色")
            .WithDescription("该角色从下一次请求起不可用，用户须重新选择角色。内置系统管理员角色不能停用。");

    private static async Task<IResult> HandleAsync([Description("角色 ID")] RoleId id, DisableRoleRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new DisableRoleCommand(id, request.ExpectedVersion, request.Reason), ct)).ToOk();
}
