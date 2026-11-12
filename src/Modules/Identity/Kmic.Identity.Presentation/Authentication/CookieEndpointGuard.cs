using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Authentication;
using Kmic.Identity.Domain.Errors;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Presentation.Authentication;

/// <summary>
/// 携带刷新 Cookie 的端点的 CSRF 防护：Origin 必须在可信名单里，并且要带自定义请求头
/// </summary>
/// <remarks>
/// 跨站请求带自定义头会触发 CORS 预检，而 CORS 只允许可信来源；再加上 Cookie 本身 SameSite=Strict，三者叠加
/// </remarks>
internal sealed class CookieEndpointGuard(IOptions<TrustedOriginsOptions> options) : IEndpointFilter
{
    public const string RequiredHeader = "X-Requested-With";
    public const string RequiredHeaderValue = "kmic";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        // 防的是浏览器自动带上的 Cookie：请求里没有刷新令牌 Cookie（移动端在请求体里给令牌）就没有什么可防的
        var request = context.HttpContext.Request;
        if (!request.Cookies.ContainsKey(RefreshCookie.Name))
        {
            return await next(context);
        }

        var origin = request.Headers.Origin.ToString();
        var trusted = options.Value.TrustedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
        var header = string.Equals(request.Headers[RequiredHeader].ToString(), RequiredHeaderValue, StringComparison.Ordinal);
        return trusted && header ? await next(context) : Result.Failure(IdentityErrors.CsrfRejected).ToNoContent();
    }
}
