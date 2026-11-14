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
    /// 审计导出不存在、类型不符或当前用户无权访问
    /// </summary>
    public static readonly Error AuditExportNotFound = Error.NotFound("Operations.AuditExportNotFound", "审计导出不存在或无权访问");

    /// <summary>
    /// 审计导出还没有完成
    /// </summary>
    public static readonly Error AuditExportNotReady = Error.Conflict("Operations.AuditExportNotReady", "审计导出还没有完成，请稍后再试").WithMetadata(ErrorMetadataKeys.Retryable, true);

    /// <summary>
    /// 审计导出失败或已取消，无结果文件
    /// </summary>
    public static readonly Error AuditExportFailed = Error.BusinessRule("Operations.AuditExportFailed", "审计导出未成功，没有可下载的文件");

    /// <summary>
    /// 定时任务不存在
    /// </summary>
    public static readonly Error JobNotFound = Error.NotFound("Operations.JobNotFound", "定时任务不存在");

    /// <summary>
    /// 定时任务正在执行
    /// </summary>
    public static readonly Error JobAlreadyRunning = Error.Conflict("Operations.JobAlreadyRunning", "任务正在执行，请等待任务结束后重试");

    /// <summary>
    /// 执行时间不是合法的 Cron 表达式，或者间隔太短
    /// </summary>
    public static readonly Error CronInvalid = Error.Validation("Operations.CronInvalid", "执行时间不合法");

    /// <summary>
    /// 返回执行时间校验失败及对应字段错误
    /// </summary>
    public static Error CronInvalidWith(string path, string message) =>
        Error.Validation(CronInvalid.Code, CronInvalid.Message, [new FieldError(path, "Cron", message)]);

    /// <summary>
    /// 后台作业不存在：模块名或作业 ID 不对
    /// </summary>
    public static readonly Error BackgroundJobNotFound = Error.NotFound("Operations.BackgroundJobNotFound", "后台作业不存在");

    /// <summary>
    /// 后台作业已结束，不能取消
    /// </summary>
    public static readonly Error BackgroundJobFinished = Error.Conflict("Operations.BackgroundJobFinished", "后台作业已结束，不能取消");

    /// <summary>
    /// 未找到可查看的死信或已放弃消息
    /// </summary>
    public static readonly Error MessageNotFound = Error.NotFound("Operations.MessageNotFound", "消息记录不存在或不可查看");

    /// <summary>
    /// 订阅路由不存在
    /// </summary>
    public static readonly Error RouteNotFound = Error.NotFound("Operations.RouteNotFound", "订阅路由不存在");

    /// <summary>
    /// 仅已在代码中声明退役的待终结订阅可终结
    /// </summary>
    public static readonly Error RouteNotRetiring = Error.Conflict("Operations.RouteNotRetiring", "仅待终结的订阅可终结，请先在代码中声明退役");

    /// <summary>
    /// 仅已终结的订阅可重新激活
    /// </summary>
    public static readonly Error RouteNotFinalized = Error.Conflict("Operations.RouteNotFinalized", "仅已终结的订阅可重新激活");

    /// <summary>
    /// 仍有心跳未过期的实例声明此订阅
    /// </summary>
    public static readonly Error RouteStillDeclared = Error.Conflict("Operations.RouteStillDeclared", "仍有心跳未过期的实例声明此订阅，请核对并停止相关实例，待心跳过期后重试");

    /// <summary>
    /// 订阅中仍有处理占用未到期的消息
    /// </summary>
    public static readonly Error RouteInFlight = Error.Conflict("Operations.RouteInFlight", "订阅中仍有消息的处理占用未到期，请稍后重试").WithMetadata(ErrorMetadataKeys.Retryable, true);

    /// <summary>
    /// 订阅还有没处理完的消息，而没有选择放弃
    /// </summary>
    /// <param name="count">没处理完的条数</param>
    public static Error RouteHasUnfinished(long count) =>
        Error.Conflict("Operations.RouteHasUnfinished", $"这个订阅还有 {count} 条没处理完的消息，选择放弃它们才能终结");

    /// <summary>
    /// 订阅已经终结，不能补投
    /// </summary>
    public static readonly Error RouteFinalizedForRedelivery = Error.Conflict("Operations.RouteFinalized", "订阅已经终结，要先重新激活再补投");

    /// <summary>
    /// 补投来源模块未注册
    /// </summary>
    public static readonly Error RedeliverSourceUnknown = Error.Validation("Operations.RedeliverSourceUnknown", "来源模块未注册");

    /// <summary>
    /// 补投的区间里消息太多
    /// </summary>
    /// <param name="max">一次最多补多少条</param>
    public static Error RedeliverTooMany(int max) =>
        Error.BusinessRule("Operations.RedeliverTooMany", $"所选区间超过单次补投上限（{max} 条），请缩小序号区间后分批补投");

    /// <summary>
    /// 订阅不存在或未声明读模型重建方式
    /// </summary>
    public static readonly Error RebuildSubscriptionUnknown = Error.NotFound("Operations.RebuildSubscriptionUnknown", "订阅不存在或未声明读模型重建方式");

    /// <summary>
    /// 订阅已有未结束的重建任务
    /// </summary>
    public static readonly Error RebuildAlreadyRunning = Error.Conflict("Operations.RebuildAlreadyRunning", "此订阅已有排队、执行中或等待确认的重建，请先处理现有重建");

    /// <summary>
    /// 订阅没有等待确认的重建任务
    /// </summary>
    public static readonly Error RebuildNotAwaitingConfirmation = Error.Conflict("Operations.RebuildNotAwaitingConfirmation", "此订阅没有等待确认的重建任务");

    /// <summary>
    /// 前端错误上报太频繁，要等一段时间；前端收到后丢弃这批，不重试
    /// </summary>
    /// <param name="retryAfter">多久以后可以再报</param>
    public static Error ClientErrorsThrottled(TimeSpan retryAfter) =>
        Error.TooManyRequests("Operations.ClientErrorsThrottled", "错误上报过于频繁，请勿重试本批数据", retryAfter);
}
