using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Authentication;
using Kmic.Identity.Domain.Errors;
using Microsoft.Extensions.Options;

namespace Kmic.Identity.Presentation.Authentication;

/// <summary>
/// 刷新 Cookie 的 CSRF 防护：校验可信 Origin 和自定义请求头
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

        // 仅检查携带刷新 Cookie 的请求；移动端通过请求体提交令牌
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
