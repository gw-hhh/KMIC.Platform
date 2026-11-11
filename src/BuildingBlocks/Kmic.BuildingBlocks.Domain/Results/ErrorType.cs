namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>
/// 错误类型
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// 输入不合法（400）
    /// </summary>
    Validation,

    /// <summary>
    /// 不存在或无权查看（404）
    /// </summary>
    NotFound,

    /// <summary>
    /// 并发、状态或唯一性冲突（409）
    /// </summary>
    Conflict,

    /// <summary>
    /// 已登录但无权执行（403）
    /// </summary>
    Forbidden,

    /// <summary>
    /// 违反业务规则（422）
    /// </summary>
    BusinessRule,

    /// <summary>
    /// 暂时不可用（503）
    /// </summary>
    Unavailable,
}
