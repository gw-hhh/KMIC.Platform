namespace Kmic.Identity.Contracts;

/// <summary>
/// Identity 模块的基本信息
/// </summary>
/// <remarks>
/// 模块名不能改：它是数据库 schema 名，也是权限点、错误码、消息名和路由的前缀
/// </remarks>
public static class IdentityModuleInfo
{
    /// <summary>
    /// 模块名
    /// </summary>
    public const string Name = "Identity";
}
