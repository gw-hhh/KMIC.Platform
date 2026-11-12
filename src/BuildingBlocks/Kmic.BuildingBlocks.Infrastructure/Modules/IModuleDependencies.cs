namespace Kmic.BuildingBlocks.Infrastructure.Modules;

/// <summary>
/// 模块声明自己依赖哪些模块
/// </summary>
public interface IModuleDependencies
{
    /// <summary>
    /// 依赖另一个模块：它必须已注册并启用，否则拒绝启动
    /// </summary>
    /// <param name="module">被依赖的模块名，取自它的 Contracts 里的 ModuleInfo.Name</param>
    void DependsOn(string module);
}
