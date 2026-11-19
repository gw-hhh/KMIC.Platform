namespace Kmic.Configuration.Application.Settings;

/// <summary>
/// 设置值里的一个成员：管理页据此生成编辑控件
/// </summary>
/// <param name="Name">属性名转为 camelCase 后的名称</param>
/// <param name="Type">类型：bool、int、long、decimal、double、string、guid、date、time、dateTime、timeSpan</param>
/// <param name="DisplayName">显示名</param>
/// <param name="Unit">单位</param>
/// <param name="IsNullable">是否可空</param>
/// <param name="IsArray">是否数组</param>
public sealed record SettingFieldInfo(string Name, string Type, string DisplayName, string? Unit, bool IsNullable, bool IsArray);
