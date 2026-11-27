namespace Kmic.BuildingBlocks.Infrastructure.Modules;

/// <summary>运维工具（迁移程序、运维命令）可识别的模块集合</summary>
public sealed class ToolModules
{
    private readonly List<(IModule Module, bool NonProductionOnly)> _modules = [];

    /// <summary>注册模块</summary>
    public ToolModules Add<TModule>(bool nonProductionOnly = false)
        where TModule : IModule, new()
    {
        _modules.Add((new TModule(), nonProductionOnly));
        return this;
    }

    /// <summary>获取当前环境需处理的模块列表（按名称排序，含配置中已禁用的模块）</summary>
    public IReadOnlyList<IModule> For(bool production) =>
    [
        .. _modules
            .Where(m => !production || !m.NonProductionOnly)
            .Select(m => m.Module)
            .OrderBy(m => m.Name, StringComparer.Ordinal),
    ];
}
