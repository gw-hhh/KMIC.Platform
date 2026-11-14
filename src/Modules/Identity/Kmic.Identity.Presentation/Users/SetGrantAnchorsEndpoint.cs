using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Users.Grants.SetAnchors;

namespace Kmic.Identity.Presentation.Users;

internal sealed class SetGrantAnchorsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/users/{id}/grants/{roleId}/anchors", HandleAsync)
            .Sends<SetGrantAnchorsCommand>()
            .WithSummary("替换角色授予的组织锚点")
            .WithDescription("不能修改自己的角色授予。目标角色受保护时，操作者的当前角色也必须受保护。所有锚点都必须是已启用的组织单元。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, [Description("角色 ID")] RoleId roleId, SetGrantAnchorsRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetGrantAnchorsCommand(id, request.ExpectedVersion, roleId, request.Anchors), ct)).ToOk();
}
