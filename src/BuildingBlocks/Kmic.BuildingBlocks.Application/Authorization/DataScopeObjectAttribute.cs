namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 标在范围对象常量上：按同一规则控制可见性的一类数据，管理员在角色上按它配置范围
/// </summary>
/// <param name="displayName">角色管理界面中的显示名</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class DataScopeObjectAttribute(string displayName) : Attribute
{
    /// <summary>
    /// 显示名
    /// </summary>
    public string DisplayName { get; } = displayName;
}
