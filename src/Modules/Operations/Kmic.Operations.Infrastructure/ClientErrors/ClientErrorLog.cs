using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.Operations.Application.ClientErrors;
using Microsoft.Extensions.Logging;

namespace Kmic.Operations.Infrastructure.ClientErrors;

internal sealed partial class ClientErrorLog(ILoggerFactory loggers) : IClientErrorLog
{
    // 日志分类固定，作观测平台筛选条件，不随类型重命名
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
