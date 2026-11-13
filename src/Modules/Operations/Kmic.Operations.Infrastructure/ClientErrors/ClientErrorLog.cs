using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Operations.Application.ClientErrors;
using Microsoft.Extensions.Logging;

namespace Kmic.Operations.Infrastructure.ClientErrors;

/// <summary>
/// 把前端上报的错误写成 Warning 级结构化日志，分类 Kmic.ClientErrors
/// </summary>
/// <remarks>
/// 不用类型作分类：观测平台上按这个固定名字筛选前端错误，类名改了名字不能跟着变。
/// 前端的消息与堆栈是用户的页面里产生的文本，当作不可信的内容原样记在字段里，不拼进消息模板
/// </remarks>
internal sealed partial class ClientErrorLog(ILoggerFactory loggers) : IClientErrorLog
{
    /// <summary>
    /// 日志分类，观测平台上的筛选条件
    /// </summary>
    public const string Category = "Kmic.ClientErrors";

    private readonly ILogger _logger = loggers.CreateLogger(Category);

    public void Write(UserId user, ClientErrorReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        Reported(_logger, user.Value, report.Type, report.Message, report.Stack, report.RouteName, report.AppVersion, report.TraceId, report.OccurredAt);
    }

    [LoggerMessage(EventId = 2601, Level = LogLevel.Warning, Message = "Client error {ClientErrorType} reported by {ReportedBy}")]
    private static partial void Reported(
        ILogger logger,
        Guid reportedBy,
        string clientErrorType,
        string clientMessage,
        string? clientStack,
        string? routeName,
        string? appVersion,
        string? clientTraceId,
        DateTimeOffset? occurredAt);
}
