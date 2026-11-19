namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 绕过数据权限：标在调用了 WithoutDataScope 的处理器上
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class BypassesDataScopeAttribute(string reason) : Attribute
{
    /// <summary>
    /// 原因
    /// </summary>
    public string Reason { get; } = reason;
}
