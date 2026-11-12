using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Settings;

namespace Kmic.Configuration.Application.Settings.Get;

/// <summary>
/// 把设置定义和值行合成列表里的一项
/// </summary>
internal static class SettingItems
{
    public static SettingItem Build(SystemSettingDefinition definition, (string Value, long Version)? stored, IAllowedActions<SettingTarget> allowedActions) =>
        new(
            definition.Key,
            definition.DisplayName,
            definition.Module,
            definition.Group,
            SettingFields.Of(definition.ValueType),
            DeclaredValues.DefaultOf(definition),
            stored is { } row ? DeclaredValues.ElementOf(row.Value) : DeclaredValues.DefaultOf(definition),
            stored is not null,
            stored?.Version,
            definition.EditPermission,
            allowedActions.For(new SettingTarget(definition.Key, definition.EditPermission)));
}
