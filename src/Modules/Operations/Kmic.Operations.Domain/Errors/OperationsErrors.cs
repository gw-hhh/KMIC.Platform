using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Domain.Errors;

/// <summary>
/// Operations 模块的错误目录
/// </summary>
/// <remarks>
/// 错误码写成“Operations.错误名”
/// </remarks>
public static class OperationsErrors
{
    /// <summary>
    /// 审计记录不存在
    /// </summary>
    public static readonly Error AuditNotFound = Error.NotFound("Operations.AuditNotFound", "审计记录不存在");

    /// <summary>
    /// 定时任务不存在
    /// </summary>
    public static readonly Error JobNotFound = Error.NotFound("Operations.JobNotFound", "定时任务不存在");

    /// <summary>
    /// 这个任务正在执行
    /// </summary>
    public static readonly Error JobAlreadyRunning = Error.Conflict("Operations.JobAlreadyRunning", "这个任务正在执行，等它结束后再试");

    /// <summary>
    /// 执行时间不是合法的 Cron 表达式，或者间隔太短
    /// </summary>
    public static readonly Error CronInvalid = Error.Validation("Operations.CronInvalid", "执行时间不合法");

    /// <summary>
    /// 执行时间不合法，并指出是哪个字段、哪里不对
    /// </summary>
    public static Error CronInvalidWith(string path, string message) =>
        Error.Validation(CronInvalid.Code, CronInvalid.Message, [new FieldError(path, "Cron", message)]);

    /// <summary>
    /// 后台作业不存在：模块名或作业 ID 不对
    /// </summary>
    public static readonly Error BackgroundJobNotFound = Error.NotFound("Operations.BackgroundJobNotFound", "后台作业不存在");

    /// <summary>
    /// 作业已经结束，不能再取消
    /// </summary>
    public static readonly Error BackgroundJobFinished = Error.Conflict("Operations.BackgroundJobFinished", "这个作业已经结束，不能取消");

    /// <summary>
    /// 这条消息不存在，或者不是死信：模块、箱、行 ID 对不上
    /// </summary>
    public static readonly Error MessageNotFound = Error.NotFound("Operations.MessageNotFound", "没有这条死信");

    /// <summary>
    /// 前端错误上报太频繁，要等一段时间；前端收到后丢弃这批，不重试
    /// </summary>
    /// <param name="retryAfter">多久以后可以再报</param>
    public static Error ClientErrorsThrottled(TimeSpan retryAfter) =>
        Error.TooManyRequests("Operations.ClientErrorsThrottled", "错误上报太频繁，请稍后再试", retryAfter);
}
