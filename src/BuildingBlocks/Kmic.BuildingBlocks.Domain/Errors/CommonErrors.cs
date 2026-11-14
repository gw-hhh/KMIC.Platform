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
    /// 依赖的数据还没同步过来，稍后自动重试
    /// </summary>
    /// <remarks>
    /// 给集成事件的处理器用：返回它会按退避重试，返回其他业务错误会直接进入死信
    /// </remarks>
    public static readonly Error NotReadyYet =
        Error.Unavailable("Common.NotReadyYet", "依赖的数据还没准备好，稍后自动重试");

    /// <summary>
    /// 同一种后台任务同时进行的太多
    /// </summary>
    public static readonly Error TooManyJobs =
        Error.Conflict("Common.TooManyJobs", "同时进行的任务太多，请等前面的完成后再提交");

    /// <summary>
    /// 作业的提交人已被禁用，或不再有这项权限
    /// </summary>
    public static readonly Error JobSubmitterUnavailable =
        Error.Forbidden("Common.JobSubmitterUnavailable", "提交人已被禁用或不再有这项权限，任务没有执行");

    /// <summary>
    /// 作业执行时出错
    /// </summary>
    public static readonly Error JobFailed =
        Error.Unavailable("Common.JobFailed", "任务执行出错，请稍后重新提交");

    /// <summary>
    /// 作业被中断，又不能续跑
    /// </summary>
    public static readonly Error JobInterrupted =
        Error.Conflict("Common.JobInterrupted", "任务被中断，请重新提交；已经完成的部分会保留");

    /// <summary>
    /// 作业超过最长运行时间
    /// </summary>
    public static readonly Error JobTimedOut =
        Error.Conflict("Common.JobTimedOut", "任务运行时间太长被停止，请缩小范围后重新提交；已经完成的部分会保留");

    /// <summary>
    /// 功能开关关闭，用例不执行：对调用方就是“这个功能不存在”
    /// </summary>
    public static readonly Error FeatureDisabled =
        Error.NotFound("Common.FeatureDisabled", "功能不存在");

    /// <summary>
    /// 导出的行数超过上限
    /// </summary>
    public static readonly Error ExportTooLarge =
        Error.Conflict("Common.ExportTooLarge", "要导出的数据太多，请缩小筛选范围后重新导出");

    /// <summary>
    /// 数据库空间不足
    /// </summary>
    public static readonly Error MessagingSuspended =
        Error.Unavailable("Common.MessagingSuspended", "系统繁忙，请稍后再试");

    /// <summary>
    /// 请求的页超出了上限计数的范围
    /// </summary>
    public static readonly Error PageOutOfRange =
        Error.Validation("Common.PageOutOfRange", "要查看的页超出了范围，请缩小筛选条件，或者改用导出");

    /// <summary>
    /// 消息类型当前版本不认识，没法确认能不能放弃
    /// </summary>
    public static readonly Error MessageUnknown =
        Error.BusinessRule("Common.MessageUnknown", "这条消息的类型当前版本不认识，不能确认能不能放弃");

    /// <summary>
    /// 消息载荷读不出来，没法确认能不能放弃
    /// </summary>
    public static readonly Error MessagePayloadUnreadable =
        Error.BusinessRule("Common.MessagePayloadUnreadable", "这条消息的载荷读不出来，不能确认能不能放弃");

    /// <summary>
    /// 文件的内容在存储里读不到：对象被删除标记覆盖或丢失，等运维还原
    /// </summary>
    public static readonly Error FileContentMissing =
        Error.Conflict("Common.FileContentMissing", "文件正在恢复，暂时读取不到，请稍后再试");

    /// <summary>
    /// 带字段错误的校验失败
    /// </summary>
    public static Error ValidationFailedWith(IReadOnlyList<FieldError> fieldErrors) =>
        Error.Validation(ValidationFailed.Code, ValidationFailed.Message, fieldErrors);
}
