namespace Kmic.BuildingBlocks.Hosting;

/// <summary>
/// 模块在哪些环境里注册
/// </summary>
public enum KmicEnvironments
{
    /// <summary>
    /// 所有环境
    /// </summary>
    Any,

    /// <summary>
    /// 只在开发与测试环境，生产环境不注册：参考模块 Sample 用
    /// </summary>
    NonProduction,
}
