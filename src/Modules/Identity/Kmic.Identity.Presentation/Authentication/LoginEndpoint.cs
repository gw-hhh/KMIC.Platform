using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Authentication;
using Kmic.Identity.Application.Authentication.Login;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Presentation.Authentication;

internal sealed class LoginEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/auth/login", HandleAsync)
            .Sends<LoginCommand>()
            .NoStore()
            .AllowAnonymous()
            .WithSummary("账号密码登录")
            .WithDescription("成功返回访问令牌。网页客户端的刷新令牌存入 HttpOnly Cookie，响应体中的 refresh_token 为 null；移动端通过响应体获取。账号不存在、密码错误或账号禁用时统一返回 401；登录尝试受限时返回 429。");

    private static async Task<IResult> HandleAsync(LoginRequest request, HttpContext http, ICommandSender sender, IOptions<SessionLifetimeOptions> sessionOptions, TimeProvider time, CancellationToken ct)
    {
        var result = await sender.SendAsync(new LoginCommand(request.Account, request.Password, request.ClientType, http.Connection.RemoteIpAddress?.ToString(), http.Request.Headers.UserAgent.ToString()), ct);
        if (result.IsSuccess && result.Value.RefreshToken is { } token && string.Equals(request.ClientType, ClientKinds.Web, StringComparison.Ordinal))
        {
            RefreshCookie.Set(http.Response, token, time.GetUtcNow() + sessionOptions.Value.WebRefreshLifetime);
            return result.Map(tokens => tokens with { RefreshToken = null }).ToOk();
        }

        return result.ToOk();
    }
}
