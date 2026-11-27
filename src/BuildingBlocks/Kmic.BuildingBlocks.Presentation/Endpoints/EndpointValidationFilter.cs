using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.UseCases;
using Microsoft.AspNetCore.Hosting;

namespace Kmic.BuildingBlocks.Presentation.Endpoints;

// 启动时校验所有模块端点声明，存在问题时拒绝启动
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
