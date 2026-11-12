using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Impersonation.End;

namespace Kmic.Identity.Presentation.Impersonation;

internal sealed class EndImpersonationEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapDelete("/impersonation", HandleAsync)
            .Sends<EndImpersonationCommand>()
            .WithSummary("结束当前的模拟会话")
            .WithDescription("用模拟令牌调用；不是模拟会话时返回 422 Identity.NotImpersonating。只读的模拟会话里也能调用。前端同时丢弃模拟令牌。");

    private static async Task<IResult> HandleAsync(ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new EndImpersonationCommand(), ct)).ToNoContent();
}
