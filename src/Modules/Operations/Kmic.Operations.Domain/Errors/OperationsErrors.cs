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
    /// 审计导出不存在：作业 ID 不对，不是审计导出作业，或者不是提交人也没有管理作业的权限
    /// </summary>
    public static readonly Error AuditExportNotFound = Error.NotFound("Operations.AuditExportNotFound", "没有这个审计导出");

    /// <summary>
    /// 审计导出还没有完成
    /// </summary>
    public static readonly Error AuditExportNotReady = Error.Conflict("Operations.AuditExportNotReady", "审计导出还没有完成，请稍后再试").WithMetadata(ErrorMetadataKeys.Retryable, true);

    /// <summary>
    /// 审计导出没有成功：失败或已取消，没有结果文件
    /// </summary>
    public static readonly Error AuditExportFailed = Error.BusinessRule("Operations.AuditExportFailed", "这次审计导出没有成功，没有可下载的文件");

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
    /// 订阅路由不存在
    /// </summary>
    public static readonly Error RouteNotFound = Error.NotFound("Operations.RouteNotFound", "订阅路由不存在");

    /// <summary>
    /// 订阅不是待终结的：要先在代码里声明它退役
    /// </summary>
    public static readonly Error RouteNotRetiring = Error.Conflict("Operations.RouteNotRetiring", "只有待终结的订阅能终结：要先在代码里声明它退役");

    /// <summary>
    /// 订阅不是已终结的，不用重新激活
    /// </summary>
    public static readonly Error RouteNotFinalized = Error.Conflict("Operations.RouteNotFinalized", "只有已终结的订阅能重新激活");

    /// <summary>
    /// 还有运行中的实例在声明这个订阅
    /// </summary>
    public static readonly Error RouteStillDeclared = Error.Conflict("Operations.RouteStillDeclared", "还有运行中的实例在声明这个订阅，等它们都停掉、新版本全部上线之后再终结");

    /// <summary>
    /// 订阅有正在处理的消息
    /// </summary>
    public static readonly Error RouteInFlight = Error.Conflict("Operations.RouteInFlight", "这个订阅有消息正在处理，等处理完再终结").WithMetadata(ErrorMetadataKeys.Retryable, true);

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
    /// 补投的来源模块没有注册
    /// </summary>
    public static readonly Error RedeliverSourceUnknown = Error.Validation("Operations.RedeliverSourceUnknown", "来源模块不对");

    /// <summary>
    /// 补投的区间里消息太多
    /// </summary>
    /// <param name="max">一次最多补多少条</param>
    public static Error RedeliverTooMany(int max) =>
        Error.BusinessRule("Operations.RedeliverTooMany", $"区间里的消息超过了一次最多补投的 {max} 条，请缩小序号区间分几次补");

    /// <summary>
    /// 订阅不存在，或者没有声明读模型的重建方式
    /// </summary>
    public static readonly Error RebuildSubscriptionUnknown = Error.NotFound("Operations.RebuildSubscriptionUnknown", "没有这个订阅，或者它没有声明读模型的重建方式");

    /// <summary>
    /// 订阅已经有没结束的一轮重建
    /// </summary>
    public static readonly Error RebuildAlreadyRunning = Error.Conflict("Operations.RebuildAlreadyRunning", "这个订阅已经有一轮重建在排队或进行，等它结束再试");

    /// <summary>
    /// 没有暂停等确认的一轮重建
    /// </summary>
    public static readonly Error RebuildNotAwaitingConfirmation = Error.Conflict("Operations.RebuildNotAwaitingConfirmation", "这个订阅没有暂停等确认的重建");

    /// <summary>
    /// 前端错误上报太频繁，要等一段时间；前端收到后丢弃这批，不重试
    /// </summary>
    /// <param name="retryAfter">多久以后可以再报</param>
    public static Error ClientErrorsThrottled(TimeSpan retryAfter) =>
        Error.TooManyRequests("Operations.ClientErrorsThrottled", "错误上报太频繁，请稍后再试", retryAfter);
}
