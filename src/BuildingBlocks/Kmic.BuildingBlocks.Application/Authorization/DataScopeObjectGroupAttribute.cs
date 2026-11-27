namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>声明数据范围对象分组的所属模块。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DataScopeObjectGroupAttribute(string module) : Attribute
{
    public string Module { get; } = module;
}
