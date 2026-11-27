namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>声明绕过数据权限过滤，标注于调用 WithoutDataScope 的处理器。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class BypassesDataScopeAttribute(string reason) : Attribute
{
    public string Reason { get; } = reason;
}
