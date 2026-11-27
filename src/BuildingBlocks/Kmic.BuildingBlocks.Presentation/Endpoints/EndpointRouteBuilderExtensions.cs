using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Presentation.Limits;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    public const string ApiV1 = "/api/v1";

    /// <summary>为已注册模块创建路由组并映射全部端点</summary>
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
                .RequireRateLimiting(RateLimitPolicies.Default)
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
