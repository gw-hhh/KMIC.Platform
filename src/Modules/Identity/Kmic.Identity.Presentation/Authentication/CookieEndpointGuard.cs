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
/// 携带刷新 Cookie 时，Origin 须在可信列表中，且 X-Requested-With 须为 kmic；Cookie 使用 SameSite=Strict
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
