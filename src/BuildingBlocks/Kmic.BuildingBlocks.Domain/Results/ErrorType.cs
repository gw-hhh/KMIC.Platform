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

    /// <summary>
    /// 凭据无效或没有登录（401）：只用于认证本身，如密码错误、刷新令牌无效
    /// </summary>
    Unauthenticated,

    /// <summary>
    /// 请求过于频繁，要等一段时间再试（429）
    /// </summary>
    TooManyRequests,

    /// <summary>
    /// 内容太大（413）：上传的文件超过了上限
    /// </summary>
    PayloadTooLarge,

    /// <summary>
    /// 不支持的内容类型（415）：上传的文件类型不被允许
    /// </summary>
    UnsupportedMediaType,

    /// <summary>
    /// 已经不存在，也不会再回来（410）：比如被彻底删除的文件
    /// </summary>
    Gone,
}
