namespace Kmic.Sample.Contracts;

/// <summary>
/// Sample 模块的基本信息
/// </summary>
/// <remarks>
/// 模块名是稳定的契约：它是数据库 schema 名，也是权限点、错误码、消息名的前缀和路由前缀
/// </remarks>
public static class SampleModuleInfo
{
    /// <summary>
    /// 模块名
    /// </summary>
    public const string Name = "Sample";
}
