using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.UseCases;
using Microsoft.AspNetCore.Hosting;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 启动时校验全部模块端点的声明，有问题就拒绝启动
/// </summary>
/// <remarks>
/// 要等管道组装完、端点都进了路由数据源才能检查，所以挂在启动过滤器上：它包着管道的组装过程，
/// 里面的 next 返回之后端点就齐了；放在后台服务里则取决于它和 Web 服务器谁先启动
/// </remarks>
internal sealed class EndpointValidationFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        ArgumentNullException.ThrowIfNull(next);

        return app =>
        {
            next(app);

            var endpoints = app.ApplicationServices.GetRequiredService<EndpointDataSource>().Endpoints
                .OfType<RouteEndpoint>()
                .Where(endpoint => endpoint.Metadata.GetMetadata<ModuleMetadata>() is not null);
            var problems = EndpointDeclarations.Check(endpoints, app.ApplicationServices.GetRequiredService<UseCaseCatalog>());
            if (problems.Count > 0)
            {
                throw new DeclarationException("端点声明", problems);
            }
        };
    }
}
