namespace Kmic.ModuleName.Contracts;

/// <summary>
/// ModuleName 模块的标识
/// </summary>
public static class ModuleNameModuleInfo
{
    /// <summary>
    /// 模块名，也是权限点、错误码、消息名的前缀，发布后不改
    /// </summary>
    public const string Name = "ModuleName";

    /// <summary>
    /// 数据库 schema 与路由前缀
    /// </summary>
    public const string Slug = "modulename";

    /// <summary>
    /// 模块编号，登记在模块登记表
    /// </summary>
    public const int Number = 99;

    /// <summary>
    /// 本模块日志 EventId 的起点，可用 EventIdBase 至 EventIdBase + 999
    /// </summary>
    public const int EventIdBase = Number * 1000;
}
