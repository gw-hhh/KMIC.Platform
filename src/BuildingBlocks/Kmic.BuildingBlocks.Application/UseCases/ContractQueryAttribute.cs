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
}
