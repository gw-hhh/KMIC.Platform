using Microsoft.AspNetCore.Hosting;

namespace Kmic.BuildingBlocks.Presentation.Authentication;

// 启动时预加载签名密钥，配置异常时拒绝启动
internal sealed class SigningKeyValidationFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        ArgumentNullException.ThrowIfNull(next);

        return app =>
        {
            _ = app.ApplicationServices.GetRequiredService<SigningKeyStore>();
            next(app);
        };
    }
}
