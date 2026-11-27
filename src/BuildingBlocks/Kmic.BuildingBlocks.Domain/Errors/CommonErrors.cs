using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Domain.Errors;

/// <summary>框架通用错误。</summary>
public static class CommonErrors
{
    /// <summary>校验失败。</summary>
    public static readonly Error ValidationFailed =
        Error.Validation("Common.ValidationFailed", "提交的数据有误");

    /// <summary>没有权限。</summary>
    public static readonly Error MissingPermission =
        Error.Forbidden("Common.MissingPermission", "没有执行此操作的权限");

    /// <summary>需要先修改密码。</summary>
    public static readonly Error PasswordChangeRequired =
        Error.Forbidden("Common.PasswordChangeRequired", "请先修改密码");

    /// <summary>只读模拟登录模式下禁止修改数据。</summary>
    public static readonly Error ImpersonationReadOnly =
        Error.Forbidden("Common.ImpersonationReadOnly", "只读模拟不能执行此操作");

    /// <summary>缺少幂等键。</summary>
    public static readonly Error IdempotencyKeyRequired =
        Error.Validation("Common.IdempotencyKeyRequired", "缺少幂等键");

    /// <summary>幂等键已被用于其他请求。</summary>
    public static readonly Error IdempotencyKeyReused =
        Error.Conflict("Common.IdempotencyKeyReused", "幂等键已用于另一个请求");

    /// <summary>相同请求正在处理中。</summary>
    public static readonly Error RequestInProgress =
        Error.Conflict("Common.RequestInProgress", "请求正在处理，请稍后再试");

    /// <summary>数据已被他人修改。</summary>
    public static readonly Error VersionConflict =
        Error.Conflict("Common.VersionConflict", "数据已被他人修改");

    /// <summary>数据重复。</summary>
    public static readonly Error DuplicateValue =
        Error.Conflict("Common.DuplicateValue", "数据重复");

    /// <summary>业务规则已变更，需重新确认。</summary>
    public static readonly Error RulesChanged =
        Error.Conflict("Common.RulesChanged", "相关规则已更新，请刷新后重新确认");

    /// <summary>数据库并发冲突（可重试）。</summary>
    public static readonly Error TransientConflict =
        Error.Unavailable("Common.TransientConflict", "系统繁忙，请稍后再试");

    /// <summary>无法确认提交是否成功。</summary>
    public static readonly Error ResultUnknown =
        Error.Unavailable("Common.ResultUnknown", "无法确认操作是否成功，请刷新后确认");

    /// <summary>依赖数据尚未同步就绪，稍后自动重试（供集成事件处理器使用；返回其他业务错误将直接进入死信队列）。</summary>
    public static readonly Error NotReadyYet =
        Error.Unavailable("Common.NotReadyYet", "依赖数据尚未就绪，稍后自动重试");

    /// <summary>同类后台作业并发数超出上限。</summary>
    public static readonly Error TooManyJobs =
        Error.Conflict("Common.TooManyJobs", "同时执行的作业过多，请等待现有作业完成后再提交");

    /// <summary>作业提交人已被禁用或已丧失执行权限。</summary>
    public static readonly Error JobSubmitterUnavailable =
        Error.Forbidden("Common.JobSubmitterUnavailable", "提交人已被禁用或已无此权限，作业未执行");

    /// <summary>作业执行失败。</summary>
    public static readonly Error JobFailed =
        Error.Unavailable("Common.JobFailed", "作业执行失败，请稍后重新提交");

    /// <summary>作业已中断且无法续跑。</summary>
    public static readonly Error JobInterrupted =
        Error.Conflict("Common.JobInterrupted", "作业已中断，请重新提交（已完成部分将保留）");

    /// <summary>作业运行超时。</summary>
    public static readonly Error JobTimedOut =
        Error.Conflict("Common.JobTimedOut", "作业超时已停止，请缩小范围后重新提交（已完成部分将保留）");

    /// <summary>功能开关已关闭（对外统一返回资源不存在）。</summary>
    public static readonly Error FeatureDisabled =
        Error.NotFound("Common.FeatureDisabled", "功能不存在");

    /// <summary>导出数据行数超出上限。</summary>
    public static readonly Error ExportTooLarge =
        Error.Conflict("Common.ExportTooLarge", "导出数据超出上限，请缩小筛选范围后重试");

    /// <summary>数据库空间不足导致消息暂停。</summary>
    public static readonly Error MessagingSuspended =
        Error.Unavailable("Common.MessagingSuspended", "系统繁忙，请稍后再试");

    /// <summary>请求页码超出最大允许范围。</summary>
    public static readonly Error PageOutOfRange =
        Error.Validation("Common.PageOutOfRange", "页码超出范围，请缩小筛选条件或改用导出");

    /// <summary>当前版本不支持该消息类型，无法判断是否可放弃。</summary>
    public static readonly Error MessageUnknown =
        Error.BusinessRule("Common.MessageUnknown", "当前版本不支持此类消息，无法判断能否放弃");

    /// <summary>无法解析消息载荷，无法判断是否可放弃。</summary>
    public static readonly Error MessagePayloadUnreadable =
        Error.BusinessRule("Common.MessagePayloadUnreadable", "无法解析消息内容，无法判断能否放弃");

    /// <summary>存储对象缺失或被删除标记覆盖，需由运维恢复。</summary>
    public static readonly Error FileContentMissing =
        Error.Conflict("Common.FileContentMissing", "文件正在恢复中，暂时无法读取，请稍后再试");

    /// <summary>带字段错误的校验失败。</summary>
    public static Error ValidationFailedWith(IReadOnlyList<FieldError> fieldErrors) =>
        Error.Validation(ValidationFailed.Code, ValidationFailed.Message, fieldErrors);
}
