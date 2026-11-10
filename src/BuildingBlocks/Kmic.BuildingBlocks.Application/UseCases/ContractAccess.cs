namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// Contracts 查询的授权语义（05-07 D3）
/// </summary>
public enum ContractAccess
{
    /// <summary>
    /// 引用解析：不查权限点、不套数据范围，结果只能是 ReferenceInfo
    /// </summary>
    Reference,

    /// <summary>
    /// 不查权限点，套用调用方在提供方范围对象上的数据范围
    /// </summary>
    CallerScoped,

    /// <summary>
    /// 只允许系统身份调用，如快照源
    /// </summary>
    SystemOnly,
}
