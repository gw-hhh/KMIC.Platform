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
            .WithDescription("只能结束当前用户的登录会话，模拟会话除外。会话不存在、属于其他用户或为模拟会话时均返回 404。结束当前会话相当于退出登录。身份校验通过后，目标会话已撤销时仍返回 204，不重复记录撤销事件。");

    private static async Task<IResult> HandleAsync([Description("会话 ID")] SessionId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RevokeMySessionCommand(id), ct)).ToNoContent();
}
