using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Authentication;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Authentication.Logout;

namespace Kmic.Identity.Presentation.Authentication;

internal sealed class LogoutEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/auth/logout", HandleAsync)
            .Sends<LogoutCommand>()
            .AllowWhenRoleUnavailable()
            .WithSummary("退出登录")
            .WithDescription("作废当前会话，成功后清除刷新令牌 Cookie。已退出的访问令牌不能再次调用此接口。");

    private static async Task<IResult> HandleAsync(HttpContext http, ICommandSender sender, CancellationToken ct)
    {
        var result = await sender.SendAsync(new LogoutCommand(), ct);
        if (result.IsSuccess)
        {
            RefreshCookie.Clear(http.Response);
        }

        return result.ToNoContent();
    }
}
