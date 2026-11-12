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
    /// 注册一个模块：Infrastructure 里的服务端注册类，和 Presentation 里的端点注册类
    /// </summary>
    /// <remarks>
    /// 两个类的模块名必须一致，启动时校验。每个模块在宿主里显式注册一次
    /// </remarks>
    /// <param name="onlyIn">模块在哪些环境里注册，默认所有环境</param>
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
