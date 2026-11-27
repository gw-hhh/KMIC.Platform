namespace Kmic.BuildingBlocks.Presentation.Endpoints;

public static class NoStoreExtensions
{
    /// <summary>为全部响应附加 Cache-Control: no-store，适用于返回令牌或临时凭据的端点</summary>
    public static TBuilder NoStore<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddEndpointFilter(async (context, next) =>
        {
            context.HttpContext.Response.Headers.CacheControl = "no-store";
            return await next(context);
        });
    }
}
