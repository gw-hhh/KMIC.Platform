namespace Kmic.Configuration.Application.Settings;

/// <summary>
/// 交给资源策略的设置定义摘要：策略不访问目录，所以键与它声明的修改权限点由处理器取好后传入
/// </summary>
/// <param name="Key">设置键</param>
/// <param name="EditPermission">声明它的模块规定的修改权限点</param>
public sealed record SettingTarget(string Key, string EditPermission);
