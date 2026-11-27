namespace Kmic.BuildingBlocks.Presentation.Http;

// 须置于管道最外层，为所有响应附加 X-Trace-Id 便于排障追踪
internal sealed class TraceIdMiddleware(RequestDelegate next)
{
    public const string Header = "X-Trace-Id";

    public Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        context.Response.OnStarting(
            static state =>
            {
                var http = (HttpContext)state;
                http.Response.Headers[Header] = KmicProblems.TraceIdOf(http);
                return Task.CompletedTask;
            },
            context);
        return next(context);
    }
}
