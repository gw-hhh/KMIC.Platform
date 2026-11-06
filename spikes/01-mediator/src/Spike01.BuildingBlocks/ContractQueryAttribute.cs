namespace Spike01.BuildingBlocks;

/// <summary>
/// 声明一个查询是 Contracts 查询，只能以 ModuleCall 来源执行
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ContractQueryAttribute(ContractAccess access) : Attribute
{
    public ContractAccess Access { get; } = access;
}
