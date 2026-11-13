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
}
