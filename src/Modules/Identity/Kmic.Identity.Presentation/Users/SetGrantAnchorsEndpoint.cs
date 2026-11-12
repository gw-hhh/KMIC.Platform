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
            .WithSummary("整体替换授予上的锚点")
            .WithDescription("规则同授予：不能改自己的，受保护角色的要求当前角色也是受保护角色；锚点必须是启用的组织单元。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, [Description("角色 ID")] RoleId roleId, SetGrantAnchorsRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetGrantAnchorsCommand(id, request.ExpectedVersion, roleId, request.Anchors), ct)).ToOk();
}
