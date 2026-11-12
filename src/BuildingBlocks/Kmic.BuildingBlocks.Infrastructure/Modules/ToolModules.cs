namespace Kmic.BuildingBlocks.Infrastructure.Modules;

/// <summary>
/// 运维程序（迁移程序、运维命令）认识的模块
/// </summary>
/// <remarks>
/// 运维程序不接请求，只需要模块的服务端注册类。它们和宿主各写一份模块清单，新建模块的脚本会一并登记；
/// 架构测试核对两份清单没有漏掉模块
/// </remarks>
public sealed class ToolModules
{
    private readonly List<(IModule Module, bool NonProductionOnly)> _modules = [];

    /// <summary>
    /// 登记模块
    /// </summary>
    /// <typeparam name="TModule">模块的服务端注册类</typeparam>
    /// <param name="nonProductionOnly">只在非生产环境：参考模块用，和宿主里的 onlyIn 一致</param>
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
    /// 配置里关闭的模块仍然处理：模块在某个环境暂时关闭，不影响它的数据库结构要保持最新
    /// </remarks>
    /// <param name="production">是否生产环境</param>
    public IReadOnlyList<IModule> For(bool production) =>
    [
        .. _modules
            .Where(m => !production || !m.NonProductionOnly)
            .Select(m => m.Module)
            .OrderBy(m => m.Name, StringComparer.Ordinal),
    ];
}
