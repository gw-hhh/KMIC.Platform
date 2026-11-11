using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Domain.Errors;

/// <summary>
/// 框架通用错误
/// </summary>
/// <remarks>
/// Error.Unavailable 只能在这里使用（KM0101）
/// </remarks>
public static class CommonErrors
{
    /// <summary>
    /// 校验失败
    /// </summary>
    public static readonly Error ValidationFailed =
        Error.Validation("Common.ValidationFailed", "提交的数据有误");

    /// <summary>
    /// 没有权限
    /// </summary>
    public static readonly Error MissingPermission =
        Error.Forbidden("Common.MissingPermission", "没有执行此操作的权限");

    /// <summary>
    /// 需要先修改密码
    /// </summary>
    public static readonly Error PasswordChangeRequired =
        Error.Forbidden("Common.PasswordChangeRequired", "请先修改密码");

    /// <summary>
    /// 只读模拟登录时不能修改数据
    /// </summary>
    public static readonly Error ImpersonationReadOnly =
        Error.Forbidden("Common.ImpersonationReadOnly", "只读模拟不能执行此操作");

    /// <summary>
    /// 缺少幂等键
    /// </summary>
    public static readonly Error IdempotencyKeyRequired =
        Error.Validation("Common.IdempotencyKeyRequired", "缺少幂等键");

    /// <summary>
    /// 幂等键被用于不同的请求
    /// </summary>
    public static readonly Error IdempotencyKeyReused =
        Error.Conflict("Common.IdempotencyKeyReused", "幂等键已用于另一个请求");

    /// <summary>
    /// 相同的请求正在处理
    /// </summary>
    public static readonly Error RequestInProgress =
        Error.Conflict("Common.RequestInProgress", "请求正在处理，请稍后再试");

    /// <summary>
    /// 数据已被他人修改
    /// </summary>
    public static readonly Error VersionConflict =
        Error.Conflict("Common.VersionConflict", "数据已被他人修改");

    /// <summary>
    /// 数据重复
    /// </summary>
    public static readonly Error DuplicateValue =
        Error.Conflict("Common.DuplicateValue", "数据重复");

    /// <summary>
    /// 规则已变化，需要重新确认
    /// </summary>
    public static readonly Error RulesChanged =
        Error.Conflict("Common.RulesChanged", "相关规则已更新，请刷新后重新确认");

    /// <summary>
    /// 数据库并发冲突，可以重试
    /// </summary>
    public static readonly Error TransientConflict =
        Error.Unavailable("Common.TransientConflict", "系统繁忙，请稍后再试");

    /// <summary>
    /// 无法确认是否提交成功
    /// </summary>
    public static readonly Error ResultUnknown =
        Error.Unavailable("Common.ResultUnknown", "无法确认操作是否成功，请刷新后确认");

    /// <summary>
    /// 数据库空间不足
    /// </summary>
    public static readonly Error MessagingSuspended =
        Error.Unavailable("Common.MessagingSuspended", "系统繁忙，请稍后再试");

    /// <summary>
    /// 带字段错误的校验失败
    /// </summary>
    public static Error ValidationFailedWith(IReadOnlyList<FieldError> fieldErrors) =>
        Error.Validation(ValidationFailed.Code, ValidationFailed.Message, fieldErrors);
}
