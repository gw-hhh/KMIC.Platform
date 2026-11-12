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
            .WithDescription("持有人正在用它的会话，下一个请求起得到 403，要重新选择角色。内置的系统管理员角色不能停用。");

    private static async Task<IResult> HandleAsync([Description("角色 ID")] RoleId id, DisableRoleRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new DisableRoleCommand(id, request.ExpectedVersion, request.Reason), ct)).ToOk();
}
