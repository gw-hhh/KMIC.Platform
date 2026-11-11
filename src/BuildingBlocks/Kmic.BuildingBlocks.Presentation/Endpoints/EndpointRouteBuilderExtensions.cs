using Kmic.BuildingBlocks.Application;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 映射模块端点
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// API 版本路由组的前缀
    /// </summary>
    public const string ApiV1 = "/api/v1";

    /// <summary>
    /// 为每个注册的模块建路由组并映射它的全部端点
    /// </summary>
    /// <remarks>
    /// 路由是 /api/v1/{模块名小写，多个单词用连字符}/{资源}。路由组统一要求登录、打上模块标签、加框架过滤器。
    /// 声明的校验在宿主启动时做，见 <see cref="EndpointValidationFilter"/>
    /// </remarks>
    /// <returns>版本路由组，宿主可以在上面追加全局设置</returns>
    public static RouteGroupBuilder MapKmicEndpoints(this IEndpointRouteBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var modules = app.ServiceProvider.GetServices<ModuleEndpoints>().ToList();
        var duplicates = modules.GroupBy(m => m.Module, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => $"模块 {g.Key} 注册了 {g.Count()} 次端点。").ToList();
        if (duplicates.Count > 0)
        {
            throw new DeclarationException("模块端点注册", duplicates);
        }

        var v1 = app.MapGroup(ApiV1);
        foreach (var module in modules)
        {
            var group = v1.MapGroup("/" + RouteNames.Segment(module.Module));
            group.RequireAuthorization()
                .WithTags(module.Module)
                .WithMetadata(new ModuleMetadata(module.Module))
                .AddEndpointFilter<ModuleEndpointFilter>();
            module.ConfigureGroup(group);

            foreach (var endpoint in module.CreateEndpoints())
            {
                endpoint.Map(group);
            }
        }

        return v1;
    }
}
