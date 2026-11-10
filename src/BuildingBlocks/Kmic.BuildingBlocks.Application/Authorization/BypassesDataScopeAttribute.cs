namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 标在使用 WithoutDataScope 的处理器上，写明为什么要看到范围之外的数据
/// </summary>
/// <remarks>
/// 用例目录据此列出全部绕过数据范围的用例，供安全审查
/// </remarks>
/// <param name="reason">绕过的原因，如“编号唯一性要在全部数据中判断”</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class BypassesDataScopeAttribute(string reason) : Attribute
{
    /// <summary>
    /// 绕过的原因
    /// </summary>
    public string Reason { get; } = reason;
}
