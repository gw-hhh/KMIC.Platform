namespace Kmic.Configuration.Application.Settings;

/// <summary>
/// 设置策略的目标；由处理器提供设置键及修改权限，策略不访问目录
/// </summary>
/// <param name="Key">设置键</param>
/// <param name="EditPermission">声明它的模块规定的修改权限点</param>
public sealed record SettingTarget(string Key, string EditPermission);
