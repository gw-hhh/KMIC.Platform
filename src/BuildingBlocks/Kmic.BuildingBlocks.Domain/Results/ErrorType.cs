namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>
/// 错误的大类，决定对外的 HTTP 状态码（映射在接口层）
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// 输入不合法（400），只有这一类可以携带字段错误
    /// </summary>
    Validation,

    /// <summary>
    /// 不存在或不可见（404）；数据范围之外的资源同样归为这一类
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
    /// 可重试的瞬时故障（503），只供框架的 CommonErrors 使用
    /// </summary>
    Unavailable,
}
