using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.UseCases;
using Microsoft.AspNetCore.Hosting;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

/// <summary>
/// 启动时校验全部模块端点的声明，有问题就拒绝启动
/// </summary>
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
