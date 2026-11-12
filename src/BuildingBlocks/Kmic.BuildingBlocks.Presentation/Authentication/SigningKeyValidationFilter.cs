using Microsoft.AspNetCore.Hosting;

namespace Kmic.BuildingBlocks.Presentation.Authentication;

/// <summary>
/// 启动时加载签名密钥，配置有问题就拒绝启动
/// </summary>
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
