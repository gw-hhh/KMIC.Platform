using Kmic.ModuleName.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Kmic.ModuleName.Presentation;

/// <summary>
/// ModuleName 模块的接口路由
/// </summary>
public static class ModuleNameEndpoints
{
    /// <summary>
    /// 本模块全部接口的路由前缀
    /// </summary>
    public const string RoutePrefix = $"/api/v1/{ModuleNameModuleInfo.Slug}";

    public static IEndpointRouteBuilder MapModuleNameEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGroup(RoutePrefix).WithTags(ModuleNameModuleInfo.Name);
        return endpoints;
    }
}
