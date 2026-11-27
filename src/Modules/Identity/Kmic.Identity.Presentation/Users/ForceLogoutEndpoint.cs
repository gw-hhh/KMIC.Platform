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
            .WithDescription("作废目标用户的指定或全部会话（含相关模拟会话）；不可对自己操作，目标持受保护角色时操作者当前角色也须受保护");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, ForceLogoutRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ForceLogoutCommand(id, request.SessionId), ct)).ToOk();
}
