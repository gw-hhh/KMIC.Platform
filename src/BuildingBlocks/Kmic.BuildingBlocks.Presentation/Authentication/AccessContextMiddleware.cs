using System.Globalization;
using System.Security.Claims;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Http;
using Microsoft.AspNetCore.Authorization;

namespace Kmic.BuildingBlocks.Presentation.Authentication;

/// <summary>
/// 令牌验证通过之后，为每个请求加载访问上下文并设置当前身份
/// </summary>
/// <remarks>
/// 令牌里只有用户、会话和当前角色，权限和数据范围每个请求从服务端加载，变更立即生效。
/// 会话已失效的令牌按没有登录处理，不是出错：匿名端点（登录、刷新）照常可用，受保护的端点由授权返回 401。
/// 要放在认证之后、授权之前，并且在路由之后：当前角色不可用时按端点的声明决定放不放行
/// </remarks>
internal sealed class AccessContextMiddleware(RequestDelegate next)
{
    public const string AuthorizationVersionHeader = "X-Authz-Version";

    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var execution = context.RequestServices.GetRequiredService<ExecutionScope>();
        var claims = ReadClaims(context.User);
        if (claims is null)
        {
            // 令牌本身有效、但缺用户、会话或角色：同样按没有登录处理，否则授权会放过它
            if (context.User.Identity is { IsAuthenticated: true })
            {
                context.User = new ClaimsPrincipal(new ClaimsIdentity());
            }

            execution.Initialize(CallOrigin.Http, CurrentUser.Anonymous);
            await next(context);
            return;
        }

        var loader = context.RequestServices.GetRequiredService<IAccessContextLoader>();
        var result = await loader.LoadForRequestAsync(claims.Value.User, claims.Value.Session, claims.Value.Role, context.RequestAborted);
        if (result.Status == AccessLoadStatus.SessionInvalid || result.Access is not { } access)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity());
            execution.Initialize(CallOrigin.Http, CurrentUser.Anonymous);
            await next(context);
            return;
        }

        execution.Initialize(CallOrigin.Http, CurrentUser.From(access), access);

        // 前端发现授权版本变了，重新拉取自己的权限；所有已认证的响应都带，包括出错的
        context.Response.OnStarting(
            static state =>
            {
                var (http, version) = ((HttpContext, long))state;
                http.Response.Headers[AuthorizationVersionHeader] = version.ToString(CultureInfo.InvariantCulture);
                return Task.CompletedTask;
            },
            (context, access.AuthorizationVersion));

        if (result.Status == AccessLoadStatus.CurrentRoleUnavailable && !IsAllowed(context.GetEndpoint()))
        {
            await RejectAsync(context);
            return;
        }

        await next(context);
    }

    // 令牌必须带用户、会话和角色，并且都是合法的 ID，缺任何一个都按没有登录处理
    private static (UserId User, SessionId Session, RoleId Role)? ReadClaims(ClaimsPrincipal principal)
    {
        if (principal.Identity is not { IsAuthenticated: true })
        {
            return null;
        }

        return Guid.TryParse(principal.FindFirstValue("sub"), out var user) && user != Guid.Empty
            && Guid.TryParse(principal.FindFirstValue("sid"), out var session) && session != Guid.Empty
            && Guid.TryParse(principal.FindFirstValue("rid"), out var role) && role != Guid.Empty
                ? (UserId.From(user), SessionId.From(session), RoleId.From(role))
                : null;
    }

    // 匿名端点（登录、刷新）和声明了放行的端点（me、switch-role）不受影响
    private static bool IsAllowed(Endpoint? endpoint) =>
        endpoint?.Metadata.GetMetadata<AllowWhenRoleUnavailableMetadata>() is not null
        || endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null;

    private static async Task RejectAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.RequestServices.GetRequiredService<IProblemDetailsService>().WriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = KmicProblems.From(FrameworkProblems.CurrentRoleUnavailable, context),
        });
    }
}
