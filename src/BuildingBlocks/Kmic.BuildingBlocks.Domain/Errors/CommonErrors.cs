using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Domain.Errors;

/// <summary>
/// 框架自身的错误目录，错误码前缀为 Common
/// </summary>
/// <remarks>
/// 只有这里可以使用 Error.Unavailable（KM0101）；各能力实现时把自己的通用错误加到这里
/// </remarks>
public static class CommonErrors
{
    /// <summary>
    /// 校验失败，字段错误在 FieldErrors 中；由校验行为附上具体的字段错误
    /// </summary>
    public static readonly Error ValidationFailed =
        Error.Validation("Common.ValidationFailed", "提交的数据有误");

    /// <summary>
    /// 当前角色没有执行该操作的权限点
    /// </summary>
    public static readonly Error MissingPermission =
        Error.Forbidden("Common.MissingPermission", "没有执行此操作的权限");

    /// <summary>
    /// 用户必须先修改密码，此前只能执行少数操作
    /// </summary>
    public static readonly Error PasswordChangeRequired =
        Error.Forbidden("Common.PasswordChangeRequired", "请先修改密码");

    /// <summary>
    /// 只读模拟会话中不能执行修改操作
    /// </summary>
    public static readonly Error ImpersonationReadOnly =
        Error.Forbidden("Common.ImpersonationReadOnly", "只读模拟不能执行此操作");

    /// <summary>
    /// 需要幂等键的操作没有提供 Idempotency-Key
    /// </summary>
    public static readonly Error IdempotencyKeyRequired =
        Error.Validation("Common.IdempotencyKeyRequired", "缺少幂等键");

    /// <summary>
    /// 同一个幂等键被用于不同的请求
    /// </summary>
    public static readonly Error IdempotencyKeyReused =
        Error.Conflict("Common.IdempotencyKeyReused", "幂等键已用于另一个请求");

    /// <summary>
    /// 同一个幂等键的请求正在处理，稍后可以用同一个键重试
    /// </summary>
    public static readonly Error RequestInProgress =
        Error.Conflict("Common.RequestInProgress", "请求正在处理，请稍后再试");

    /// <summary>
    /// 数据已被他人修改；由异常转换附上数据库中的最新版本
    /// </summary>
    public static readonly Error VersionConflict =
        Error.Conflict("Common.VersionConflict", "数据已被他人修改");

    /// <summary>
    /// 违反了没有登记映射的唯一约束
    /// </summary>
    public static readonly Error DuplicateValue =
        Error.Conflict("Common.DuplicateValue", "数据重复");

    /// <summary>
    /// 操作所依据的规则已变化，需要重新确认；为将来的版本化规则集预留
    /// </summary>
    public static readonly Error RulesChanged =
        Error.Conflict("Common.RulesChanged", "相关规则已更新，请刷新后重新确认");

    /// <summary>
    /// 死锁或序列化失败，可以用同一个幂等键重试
    /// </summary>
    public static readonly Error TransientConflict =
        Error.Unavailable("Common.TransientConflict", "系统繁忙，请稍后再试");

    /// <summary>
    /// 提交结果无法判定：幂等命令必须用原键重试，其他命令请刷新后确认
    /// </summary>
    public static readonly Error ResultUnknown =
        Error.Unavailable("Common.ResultUnknown", "无法确认操作是否成功，请刷新后确认");

    /// <summary>
    /// 数据库空间接近上限，暂停会产生消息的写入
    /// </summary>
    public static readonly Error MessagingSuspended =
        Error.Unavailable("Common.MessagingSuspended", "系统繁忙，请稍后再试");

    /// <summary>
    /// 带字段错误的校验失败；由校验行为调用
    /// </summary>
    public static Error ValidationFailedWith(IReadOnlyList<FieldError> fieldErrors) =>
        Error.Validation(ValidationFailed.Code, ValidationFailed.Message, fieldErrors);
}
