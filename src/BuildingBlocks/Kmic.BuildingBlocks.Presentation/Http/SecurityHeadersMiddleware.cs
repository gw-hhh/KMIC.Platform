namespace Kmic.BuildingBlocks.Presentation.Http;

// 须置于错误处理中间件之前，确保错误响应也带安全头；不覆盖已显式设置的头
internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    private const string ApiPrefix = "/api/";

    // HSTS 有效期一年，省略 includeSubDomains 以免影响同主域下的其他系统
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

                // API 默认返回用户态数据，禁止浏览器及中间代理缓存；需协商缓存的端点（如带 ETag 的字典）会自行设置 Cache-Control
                if (http.Request.Path.StartsWithSegments(ApiPrefix.TrimEnd('/'), StringComparison.Ordinal))
                {
                    headers.TryAdd("Cache-Control", "no-store");
                }

                // 经转发头中间件处理后，IsHttps 即反映客户端实际访问协议
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
