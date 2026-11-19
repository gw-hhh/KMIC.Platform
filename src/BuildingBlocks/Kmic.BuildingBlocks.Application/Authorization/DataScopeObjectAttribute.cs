namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 数据范围对象
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class DataScopeObjectAttribute(string displayName) : Attribute
{
    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; } = displayName;
}
