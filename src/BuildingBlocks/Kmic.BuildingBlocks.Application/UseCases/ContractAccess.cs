namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// Contracts 查询的授权方式
/// </summary>
public enum ContractAccess
{
    /// <summary>
    /// 引用解析：不检查权限点和数据权限，只能返回 ReferenceInfo
    /// </summary>
    Reference,

    /// <summary>
    /// 不检查权限点，按调用方的数据权限过滤
    /// </summary>
    CallerScoped,

    /// <summary>
    /// 只允许系统身份调用
    /// </summary>
    SystemOnly,
}
