namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 绕过数据范围：标在调用了 WithoutDataScope 的处理器上，或手写 SQL 查带数据范围的表却不加范围条件的类型上
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class BypassesDataScopeAttribute(string reason) : Attribute
{
    /// <summary>
    /// 原因
    /// </summary>
    public string Reason { get; } = reason;
}
