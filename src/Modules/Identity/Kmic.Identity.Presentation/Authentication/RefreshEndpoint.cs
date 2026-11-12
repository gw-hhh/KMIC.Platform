using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Authentication;
using Kmic.Identity.Application.Authentication.Refresh;
using Kmic.Identity.Domain.Errors;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Presentation.Authentication;

internal sealed class RefreshEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/auth/refresh", HandleAsync)
            .Sends<RefreshTokenCommand>()
            .AllowAnonymous()
            .AddEndpointFilter<CookieEndpointGuard>()
            .WithSummary("刷新令牌")
            .WithDescription("网页客户端从 HttpOnly Cookie 取刷新令牌（要求可信 Origin 与 X-Requested-With: kmic），移动端在请求体里给出 refreshToken。每次刷新换一个新的刷新令牌；旧令牌宽限期之外再次出现，整个会话作废并返回 401。");

    private static async Task<IResult> HandleAsync(
        RefreshRequest? request,
        HttpContext http,
        ICommandSender sender,
        IOptions<SessionLifetimeOptions> sessionOptions,
        TimeProvider time,
        CancellationToken ct)
    {
        var cookie = http.Request.Cookies[RefreshCookie.Name];
        var presented = cookie ?? request?.RefreshToken;
        if (string.IsNullOrWhiteSpace(presented))
        {
            return Result.Failure(IdentityErrors.InvalidRefreshToken).ToNoContent();
        }

        var result = await sender.SendAsync(new RefreshTokenCommand(presented, http.Connection.RemoteIpAddress?.ToString()), ct);
        if (result.IsFailure && string.Equals(result.Error.Code, IdentityErrors.RefreshTokenReused.Code, StringComparison.Ordinal))
        {
            // 重用：刷新命令已经回滚，另发命令在独立事务里作废会话；对外和其他刷新失败一样，Cookie 一并清掉
            await sender.SendAsync(new RevokeReusedSessionCommand(presented), ct);
            RefreshCookie.Clear(http.Response);
            return Result.Failure(IdentityErrors.InvalidRefreshToken).ToNoContent();
        }

        if (result.IsSuccess && cookie is not null && result.Value.RefreshToken is { } next)
        {
            RefreshCookie.Set(http.Response, next, time.GetUtcNow() + sessionOptions.Value.WebRefreshLifetime);
            return result.Map(tokens => tokens with { RefreshToken = null }).ToOk();
        }

        return result.ToOk();
    }
}
