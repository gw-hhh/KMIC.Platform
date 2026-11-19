namespace Kmic.BuildingBlocks.Infrastructure.Modules;

/// <summary>
/// 运维程序（迁移程序、运维命令）认识的模块
/// </summary>
public sealed class ToolModules
{
    private readonly List<(IModule Module, bool NonProductionOnly)> _modules = [];

    /// <summary>
    /// 登记模块
    /// </summary>
    /// <typeparam name="TModule">模块的服务端注册类</typeparam>
    public ToolModules Add<TModule>(bool nonProductionOnly = false)
        where TModule : IModule, new()
    {
        _modules.Add((new TModule(), nonProductionOnly));
        return this;
    }

    /// <summary>
    /// 当前环境要处理的模块，按名称排序
    /// </summary>
    /// <remarks>
    /// 配置里关闭的模块仍然处理
    /// </remarks>
    public IReadOnlyList<IModule> For(bool production) =>
    [
        .. _modules
            .Where(m => !production || !m.NonProductionOnly)
            .Select(m => m.Module)
            .OrderBy(m => m.Name, StringComparer.Ordinal),
    ];
}
