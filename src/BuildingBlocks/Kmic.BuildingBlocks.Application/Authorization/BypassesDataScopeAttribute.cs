namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 绕过数据权限
/// </summary>
/// <remarks>
/// 标在调用了 WithoutDataScope 的处理器上，安全审查时按它列出这些用例
/// </remarks>
/// <param name="reason">原因，如“编号要在全部数据中判断是否重复”</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class BypassesDataScopeAttribute(string reason) : Attribute
{
    /// <summary>
    /// 原因
    /// </summary>
    public string Reason { get; } = reason;
}
