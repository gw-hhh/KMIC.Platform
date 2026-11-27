using Kmic.BuildingBlocks.Application.Execution;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

internal sealed class ModuleEndpointFilter : IEndpointFilter
{
    public const string IdempotencyKeyHeader = "Idempotency-Key";
    public const string ReplayedHeader = "Idempotent-Replayed";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var http = context.HttpContext;
        var metadata = http.RequestServices.GetRequiredService<RequestMetadata>();

        // 同名请求头出现多次时无法确定有效键值，视作无效幂等键交由幂等管道拦截
        if (http.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var values))
        {
            metadata.IdempotencyKey = values.Count == 1 ? values[0] : string.Empty;
        }

        var result = await next(context);

        if (metadata.Replayed)
        {
            http.Response.Headers[ReplayedHeader] = "true";
        }

        return result;
    }
}
