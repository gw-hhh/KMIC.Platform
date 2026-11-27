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
            .NoStore()
            .AllowAnonymous()
            .AddEndpointFilter<CookieEndpointGuard>()
            .WithSummary("刷新令牌")
            .WithDescription("Web 端通过 HttpOnly Cookie 提交（须带可信 Origin 与 X-Requested-With: kmic），移动端传 refreshToken；成功后轮换刷新令牌，宽限期外重用旧令牌则作废会话并返回 401");

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
            // 独立提交会话作废操作并清除 Cookie，对外统一返回刷新失败
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
