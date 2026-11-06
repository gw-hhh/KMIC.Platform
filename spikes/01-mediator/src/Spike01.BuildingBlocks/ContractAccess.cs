namespace Spike01.BuildingBlocks;

/// <summary>
/// Contracts 查询的授权语义（05-07 D3）
/// </summary>
public enum ContractAccess
{
    Reference,
    CallerScoped,
    SystemOnly,
}
