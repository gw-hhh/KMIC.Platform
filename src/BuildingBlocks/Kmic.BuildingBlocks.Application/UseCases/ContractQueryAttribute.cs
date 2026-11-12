namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 模块间查询
/// </summary>
/// <param name="access">授权方式</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ContractQueryAttribute(ContractAccess access) : Attribute
{
    /// <summary>
    /// 授权方式
    /// </summary>
    public ContractAccess Access { get; } = access;

    /// <summary>
    /// 引用解析查询的返回类型不是 ReferenceInfo（或它的集合、字典）时，在这里写明原因
    /// </summary>
    /// <remarks>
    /// 引用解析不受调用方的权限和数据范围限制，所以返回内容被限定在 ReferenceInfo，免得它变成读取别的模块数据的通道。
    /// 少数查询确实需要返回别的类型（候选人员、字典候选值、文件描述），逐个写明为什么可以。只能用在 Reference 查询上
    /// </remarks>
    public string? ResultExemption { get; init; }
}
