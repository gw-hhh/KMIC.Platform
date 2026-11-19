namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>
/// 给所有响应加安全响应头
/// </summary>
/// <remarks>
/// 要放在错误处理之前，出错的响应也带；已经设置过的头不覆盖
/// </remarks>
internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    private const string ApiPrefix = "/api/";

    // 一年；不带 includeSubDomains，免得影响同一主域名下的其他系统
    private const string HstsValue = "max-age=31536000";

    public Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Response.OnStarting(
            static state =>
            {
                var http = (HttpContext)state;
                var headers = http.Response.Headers;
                headers.TryAdd("X-Content-Type-Options", "nosniff");
                headers.TryAdd("X-Frame-Options", "DENY");
                headers.TryAdd("Referrer-Policy", "no-referrer");

                // 接口返回的是登录后的数据，不让浏览器和中间代理缓存；要缓存的端点（如带 ETag 的字典）自己设了 Cache-Control
                if (http.Request.Path.StartsWithSegments(ApiPrefix.TrimEnd('/'), StringComparison.Ordinal))
                {
                    headers.TryAdd("Cache-Control", "no-store");
                }

                // 转发头处理之后 IsHttps 反映的是用户的真实协议
                if (http.Request.IsHttps)
                {
                    headers.TryAdd("Strict-Transport-Security", HstsValue);
                }

                return Task.CompletedTask;
            },
            context);
        return next(context);
    }
}
