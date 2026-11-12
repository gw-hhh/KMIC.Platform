using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Authentication.MySessions;

namespace Kmic.Identity.Presentation.Authentication;

internal sealed class RevokeMySessionEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapDelete("/me/sessions/{id}", HandleAsync)
            .Sends<RevokeMySessionCommand>()
            .WithSummary("结束我的一个登录设备")
            .WithDescription("只能结束自己的会话，别人的会话返回 404。结束当前会话等于退出登录。天然幂等。");

    private static async Task<IResult> HandleAsync([Description("会话 ID")] SessionId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RevokeMySessionCommand(id), ct)).ToNoContent();
}
