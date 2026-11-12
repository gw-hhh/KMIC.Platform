using Kmic.BuildingBlocks.Application;
using Kmic.BuildingBlocks.Application.Access;
using Microsoft.AspNetCore.Hosting;

namespace Kmic.BuildingBlocks.Hosting;

/// <summary>
/// 启动时确认注册了访问上下文加载器，没有就拒绝启动
/// </summary>
internal sealed class AccessContextLoaderFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        ArgumentNullException.ThrowIfNull(next);

        return app =>
        {
            if (app.ApplicationServices.GetService<IAccessContextLoader>() is null)
            {
                throw new DeclarationException("宿主", ["Api 角色需要 IAccessContextLoader：由 Identity 模块提供，没有它就无法确定每个请求是谁发的。"]);
            }

            next(app);
        };
    }
}
