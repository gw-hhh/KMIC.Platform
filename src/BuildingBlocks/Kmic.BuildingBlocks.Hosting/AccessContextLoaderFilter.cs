using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Access;
using Microsoft.AspNetCore.Hosting;

namespace Kmic.BuildingBlocks.Hosting;

internal sealed class AccessContextLoaderFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        ArgumentNullException.ThrowIfNull(next);

        return app =>
        {
            if (app.ApplicationServices.GetService<IAccessContextLoader>() is null)
            {
                throw new DeclarationException("宿主", ["Api 角色依赖 IAccessContextLoader（由 Identity 模块提供），缺少时无法识别请求身份。"]);
            }

            next(app);
        };
    }
}
