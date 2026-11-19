namespace Kmic.BuildingBlocks.Infrastructure.Modules;

/// <summary>
/// 模块声明自己依赖哪些模块
/// </summary>
public interface IModuleDependencies
{
    /// <summary>
    /// 依赖另一个模块：它必须已注册并启用，否则拒绝启动
    /// </summary>
    void DependsOn(string module);
}
