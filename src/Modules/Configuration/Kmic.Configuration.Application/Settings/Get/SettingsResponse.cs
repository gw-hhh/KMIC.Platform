namespace Kmic.Configuration.Application.Settings.Get;

/// <summary>
/// 全部系统设置
/// </summary>
/// <param name="Settings">代码里声明的设置，按模块、分组、键排好</param>
/// <param name="Obsolete">代码里已经不存在、值行还留着的设置</param>
public sealed record SettingsResponse(IReadOnlyList<SettingItem> Settings, IReadOnlyList<ObsoleteSettingItem> Obsolete);
