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
    /// 用在响应体里有令牌、一次性临时密码这类凭据的端点上（登录、刷新、切换角色、重置密码）。
    /// 访问令牌的规范（RFC 6749）要求这样做；浏览器的“后退”和共享代理也不应该留下这些响应
    /// </remarks>
    /// <param name="builder">端点</param>
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
