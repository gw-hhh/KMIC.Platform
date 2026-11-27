namespace Kmic.BuildingBlocks.Hosting;

/// <summary>模块允许注册的运行环境范围。</summary>
public enum KmicEnvironments
{
    /// <summary>所有环境。</summary>
    Any,

    /// <summary>仅在开发与测试环境注册，生产环境不注册（供参考模块 Sample 使用）。</summary>
    NonProduction,
}
