namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>声明数据范围对象的显示名称。</summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class DataScopeObjectAttribute(string displayName) : Attribute
{
    public string DisplayName { get; } = displayName;
}
