namespace Kmic.BuildingBlocks.Presentation.Http;

/// <summary>
/// 所有响应都带 X-Trace-Id，用户报障时提供它
/// </summary>
/// <remarks>
/// 放在最外层。用响应开始时的回调写头：异常处理会清掉已设置的响应头，401、404、500 也要带
/// </remarks>
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
