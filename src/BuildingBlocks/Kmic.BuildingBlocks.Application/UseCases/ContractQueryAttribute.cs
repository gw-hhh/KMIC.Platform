namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 模块间查询
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ContractQueryAttribute(ContractAccess access) : Attribute
{
    /// <summary>
    /// 授权方式
    /// </summary>
    public ContractAccess Access { get; } = access;

    /// <summary>
    /// 引用解析查询的返回类型不是 ReferenceInfo 时，写明原因
    /// </summary>
    /// <remarks>
    /// 只能用在 Reference 查询上
    /// </remarks>
    public string? ResultExemption { get; init; }
}
