namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 数据范围对象分组
/// </summary>
/// <param name="module">模块名</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DataScopeObjectGroupAttribute(string module) : Attribute
{
    /// <summary>
    /// 模块名
    /// </summary>
    public string Module { get; } = module;
}
