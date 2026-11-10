namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 模块间 Contracts 查询，只能以 ModuleCall 来源执行
/// </summary>
/// <param name="access">授权语义</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ContractQueryAttribute(ContractAccess access) : Attribute
{
    /// <summary>
    /// 授权语义
    /// </summary>
    public ContractAccess Access { get; } = access;
}
