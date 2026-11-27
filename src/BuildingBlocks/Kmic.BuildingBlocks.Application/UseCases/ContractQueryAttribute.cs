namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>声明模块间契约查询的授权方式。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ContractQueryAttribute(ContractAccess access) : Attribute
{
    public ContractAccess Access { get; } = access;

    /// <summary>引用解析豁免原因（仅限 Reference 类型查询，返回类型非 ReferenceInfo 时须说明）。</summary>
    public string? ResultExemption { get; init; }
}
