using System.Globalization;
using System.Security.Claims;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Http;
using Microsoft.AspNetCore.Authorization;

namespace Kmic.BuildingBlocks.Presentation.Authentication;

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
            // 令牌有效但缺少用户、会话或角色声明时同样按未登录处理，避免授权阶段误放行
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

        // 前端检测到授权版本变化时重新拉取权限；所有已认证响应（含错误响应）均需携带
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

    // 令牌须包含有效的用户、会话与角色 ID，缺少任一声明均按未登录处理
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

    // 允许匿名的端点（如登录、刷新）及显式放行端点（如 me、switch-role）不受此限制
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
