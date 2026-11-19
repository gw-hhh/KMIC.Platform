namespace Kmic.Configuration.Application.Settings.Get;

/// <summary>
/// 全部系统设置
/// </summary>
/// <param name="Settings">代码里声明的设置</param>
/// <param name="Obsolete">代码中已移除但仍有保存值的设置</param>
public sealed record SettingsResponse(IReadOnlyList<SettingItem> Settings, IReadOnlyList<ObsoleteSettingItem> Obsolete);
