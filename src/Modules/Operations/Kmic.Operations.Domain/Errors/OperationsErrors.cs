using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Domain.Errors;

/// <summary>
/// Operations 模块的错误目录
/// </summary>
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
    /// 定时任务正在执行或已在排队
    /// </summary>
    public static readonly Error JobAlreadyRunning = Error.Conflict("Operations.JobAlreadyRunning", "任务正在执行或已在排队，请等任务结束后再试");

    /// <summary>
    /// Cron 校验失败，具体原因见字段错误
    /// </summary>
    public static readonly Error CronInvalid = Error.Validation("Operations.CronInvalid", "Cron 表达式不符合要求");

    /// <summary>
    /// 返回指定字段的 Cron 校验错误
    /// </summary>
    public static Error CronInvalidWith(string path, string message) =>
        Error.Validation(CronInvalid.Code, CronInvalid.Message, [new FieldError(path, "Cron", message)]);

    /// <summary>
    /// 未找到可访问的后台作业
    /// </summary>
    public static readonly Error BackgroundJobNotFound = Error.NotFound("Operations.BackgroundJobNotFound", "后台作业不存在");

    /// <summary>
    /// 作业已结束或正在取消，不能再次取消
    /// </summary>
    public static readonly Error BackgroundJobFinished = Error.Conflict("Operations.BackgroundJobFinished", "作业已结束或正在取消，不能再次取消");

    /// <summary>
    /// 未找到可查看的死信或已放弃消息
    /// </summary>
    public static readonly Error MessageNotFound = Error.NotFound("Operations.MessageNotFound", "未找到死信或已放弃的消息");

    /// <summary>
    /// 订阅路由不存在
    /// </summary>
    public static readonly Error RouteNotFound = Error.NotFound("Operations.RouteNotFound", "订阅路由不存在");

    /// <summary>
    /// 订阅不处于待终结状态
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
    /// 仍有处于处理中且占用未到期的消息
    /// </summary>
    public static readonly Error RouteInFlight = Error.Conflict("Operations.RouteInFlight", "仍有消息被占用，暂不能终结订阅，请稍后重试").WithMetadata(ErrorMetadataKeys.Retryable, true);

    /// <summary>
    /// 订阅仍有未完成消息，且未选择放弃
    /// </summary>
    public static Error RouteHasUnfinished(long count) =>
        Error.Conflict("Operations.RouteHasUnfinished", $"订阅还有 {count} 条未完成消息，请先处理完成，或选择放弃后再终结");

    /// <summary>
    /// 订阅已经终结，不能补投
    /// </summary>
    public static readonly Error RouteFinalizedForRedelivery = Error.Conflict("Operations.RouteFinalized", "订阅已经终结，要先重新激活再补投");

    /// <summary>
    /// 补投来源模块未注册
    /// </summary>
    public static readonly Error RedeliverSourceUnknown = Error.Validation("Operations.RedeliverSourceUnknown", "来源模块未注册");

    /// <summary>
    /// 补投消息数量超过单次上限
    /// </summary>
    public static Error RedeliverTooMany(int max) =>
        Error.BusinessRule("Operations.RedeliverTooMany", $"所选区间内的消息超过单次补投上限（{max} 条），请缩小区间后分批补投");

    /// <summary>
    /// 订阅不存在或未声明读模型重建方式
    /// </summary>
    public static readonly Error RebuildSubscriptionUnknown = Error.NotFound("Operations.RebuildSubscriptionUnknown", "订阅不存在或未声明读模型重建方式");

    /// <summary>
    /// 订阅已有未结束的重建任务
    /// </summary>
    public static readonly Error RebuildAlreadyRunning = Error.Conflict("Operations.RebuildAlreadyRunning", "该订阅已有未结束的重建任务，请先处理现有任务");

    /// <summary>
    /// 订阅没有等待确认的重建任务
    /// </summary>
    public static readonly Error RebuildNotAwaitingConfirmation = Error.Conflict("Operations.RebuildNotAwaitingConfirmation", "此订阅没有等待确认的重建任务");

    /// <summary>
    /// 错误上报超限；丢弃本批数据，等待后再上报新数据
    /// </summary>
    public static Error ClientErrorsThrottled(TimeSpan retryAfter) =>
        Error.TooManyRequests("Operations.ClientErrorsThrottled", "错误上报过于频繁，请勿重试本批数据", retryAfter);
}
