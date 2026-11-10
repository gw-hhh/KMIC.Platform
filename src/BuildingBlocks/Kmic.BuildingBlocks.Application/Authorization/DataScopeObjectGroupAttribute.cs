namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 标在模块的范围对象常量类上
/// </summary>
/// <param name="module">模块名，范围对象编码的第一段必须与它相同</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DataScopeObjectGroupAttribute(string module) : Attribute
{
    /// <summary>
    /// 模块名
    /// </summary>
    public string Module { get; } = module;
}
