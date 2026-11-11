using Kmic.BuildingBlocks.Application.Execution;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 模块路由组上的框架过滤器
/// </summary>
/// <remarks>
/// 请求头 Idempotency-Key 交给管道里的幂等行为；幂等重放时在响应上加 Idempotent-Replayed。
/// 状态码与首次相同，由命令的结果自然带出，这里只管头
/// </remarks>
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

        // 同名的头出现两次时无法确定哪个才是键，按没有有效的键处理，由幂等行为拒绝
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
