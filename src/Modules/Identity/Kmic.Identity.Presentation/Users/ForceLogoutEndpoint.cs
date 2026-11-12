using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Users.ForceLogout;

namespace Kmic.Identity.Presentation.Users;

internal sealed class ForceLogoutEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/users/{id}/force-logout", HandleAsync)
            .Sends<ForceLogoutCommand>()
            .WithSummary("强制下线")
            .WithDescription("指定的一个会话，或该用户的全部会话（含他发起的模拟会话）。不能对自己用，自己的登录设备在“我的登录设备”里管理。持有受保护角色的用户，只有同样持有受保护角色的人才能用。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, ForceLogoutRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ForceLogoutCommand(id, request.SessionId), ct)).ToOk();
}
