namespace Kmic.BuildingBlocks.Infrastructure.Modules;

/// <summary>模块依赖声明接口</summary>
public interface IModuleDependencies
{
    /// <summary>声明依赖的目标模块；目标模块须已注册且启用，否则拒绝启动</summary>
    void DependsOn(string module);
}
