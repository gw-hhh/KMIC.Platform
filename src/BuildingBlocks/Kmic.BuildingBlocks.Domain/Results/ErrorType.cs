namespace Kmic.BuildingBlocks.Domain.Results;

/// <summary>错误类型。</summary>
public enum ErrorType
{
    /// <summary>参数校验失败（HTTP 400）。</summary>
    Validation,

    /// <summary>资源不存在或无权查看（HTTP 404）。</summary>
    NotFound,

    /// <summary>并发、状态或唯一性冲突（HTTP 409）。</summary>
    Conflict,

    /// <summary>已认证但无执行权限（HTTP 403）。</summary>
    Forbidden,

    /// <summary>违反业务规则（HTTP 422）。</summary>
    BusinessRule,

    /// <summary>服务暂时不可用（HTTP 503）。</summary>
    Unavailable,

    /// <summary>未认证或凭据无效（HTTP 401）。</summary>
    Unauthenticated,

    /// <summary>请求过于频繁，需等待后重试（HTTP 429）。</summary>
    TooManyRequests,

    /// <summary>请求内容过大，超出允许上限（HTTP 413）。</summary>
    PayloadTooLarge,

    /// <summary>不支持的内容类型（HTTP 415）。</summary>
    UnsupportedMediaType,

    /// <summary>资源已永久移除（HTTP 410）。</summary>
    Gone,
}
