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
            .WithDescription("返回访问令牌：Web 端刷新令牌写入 HttpOnly Cookie（响应体为 null），移动端直接由响应体返回；凭据无效或禁用统一返回 401，触发限流返回 429");

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
