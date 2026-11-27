namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>声明绕过数据范围校验，标注于调用 WithoutDataScope 或手写 SQL 查询受控表但无需范围过滤的类型。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class BypassesDataScopeAttribute(string reason) : Attribute
{
    public string Reason { get; } = reason;
}
