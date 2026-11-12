namespace Kmic.Configuration.Application.Settings;

/// <summary>
/// 设置值里的一个成员：管理页据此生成编辑控件
/// </summary>
/// <param name="Name">成员名，与 JSON 里的字段名一致</param>
/// <param name="Type">类型：bool、int、long、decimal、double、string、guid、date、time、dateTime、timeSpan</param>
/// <param name="DisplayName">显示名；没有声明时是成员名</param>
/// <param name="Unit">单位</param>
/// <param name="IsNullable">是否可空</param>
/// <param name="IsArray">是否数组</param>
public sealed record SettingFieldInfo(string Name, string Type, string DisplayName, string? Unit, bool IsNullable, bool IsArray);
