using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Presentation.Endpoints;

namespace Kmic.BuildingBlocks.Hosting;

/// <summary>
/// 登记宿主要运行的模块
/// </summary>
public sealed class KmicHostBuilder
{
    private readonly List<HostModule> _modules = [];

    internal IReadOnlyList<HostModule> Modules => _modules;

    /// <summary>
    /// 注册一个模块：服务端注册类和端点注册类
    /// </summary>
    /// <typeparam name="TModule">服务端注册类</typeparam>
    /// <typeparam name="TEndpoints">端点注册类</typeparam>
    public KmicHostBuilder AddModule<TModule, TEndpoints>(KmicEnvironments onlyIn = KmicEnvironments.Any)
        where TModule : IModule, new()
        where TEndpoints : ModuleEndpoints, new()
    {
        _modules.Add(new HostModule(new TModule(), new TEndpoints(), onlyIn));
        return this;
    }
}
