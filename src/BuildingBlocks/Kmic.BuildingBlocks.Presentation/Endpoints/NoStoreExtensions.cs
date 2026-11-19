namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 响应里带着凭据的端点：不让浏览器和中间的代理缓存
/// </summary>
public static class NoStoreExtensions
{
    /// <summary>
    /// 响应带 <c>Cache-Control: no-store</c>，成功和失败都带
    /// </summary>
    /// <remarks>
    /// 响应体里有令牌、临时密码这类凭据的端点要用
    /// </remarks>
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
