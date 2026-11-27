using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Presentation.Endpoints;

namespace Kmic.BuildingBlocks.Hosting;

/// <summary>宿主模块注册构建器。</summary>
public sealed class KmicHostBuilder
{
    private readonly List<HostModule> _modules = [];

    internal IReadOnlyList<HostModule> Modules => _modules;

    /// <summary>注册模块及其对应的服务端注册类与端点注册类。</summary>
    public KmicHostBuilder AddModule<TModule, TEndpoints>(KmicEnvironments onlyIn = KmicEnvironments.Any)
        where TModule : IModule, new()
        where TEndpoints : ModuleEndpoints, new()
    {
        _modules.Add(new HostModule(new TModule(), new TEndpoints(), onlyIn));
        return this;
    }
}
