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
            .WithSummary("结束当前用户的登录会话")
            .WithDescription("作废本人非模拟登录会话（不存在、属他人或模拟会话返回 404）；目标已撤销时幂等返回 204");

    private static async Task<IResult> HandleAsync([Description("会话 ID")] SessionId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RevokeMySessionCommand(id), ct)).ToNoContent();
}
