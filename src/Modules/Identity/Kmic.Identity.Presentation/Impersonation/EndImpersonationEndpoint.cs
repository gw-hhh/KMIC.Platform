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
            .WithSummary("结束当前模拟会话")
            .WithDescription("仅限模拟令牌调用（非模拟会话返回 422 Identity.NotImpersonating），只读模拟也可调用；结束后前端须丢弃模拟令牌");

    private static async Task<IResult> HandleAsync(ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new EndImpersonationCommand(), ct)).ToNoContent();
}
