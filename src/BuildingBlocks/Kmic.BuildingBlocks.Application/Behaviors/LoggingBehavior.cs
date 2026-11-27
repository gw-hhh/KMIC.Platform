using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Application.Behaviors;

/// <summary>记录请求日志与耗时指标，不记录消息载荷。</summary>
public sealed class LoggingBehavior<TMessage, TResponse>(
    UseCaseCatalog catalog,
    IExecutionContext context,
    TimeProvider time,
    FrameworkMetrics metrics,
    IOptions<PipelineOptions> options,
    ILogger<LoggingBehavior<TMessage, TResponse>> logger) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : Result
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var useCase = catalog.Get(typeof(TMessage));
        using var logScope = logger.BeginScope(LogProperties(useCase));
        using var activity = KmicActivities.Messaging.StartActivity(useCase.Name);
        activity?.SetTag("kmic.message.kind", useCase.Kind == MessageKind.Command ? "command" : "query");
        activity?.SetTag("kmic.origin", context.Origin.ToString());

        var started = time.GetTimestamp();
        var outcome = "exception";
        try
        {
            var response = await next(message, cancellationToken);
            outcome = response.IsSuccess ? "success" : response.Error.Code;
            return response;
        }
        finally
        {
            var elapsed = time.GetElapsedTime(started);
            activity?.SetTag("kmic.result", outcome);
            metrics.Request(useCase.Module, useCase.Name, outcome, elapsed);
            var user = context.User;
            if (elapsed > options.Value.SlowMessageThreshold)
            {
                LoggingBehaviorLog.SlowMessage(logger, useCase.Name, outcome, elapsed.TotalMilliseconds, user.UserId?.Value, user.RoleId?.Value);
            }
            else
            {
                LoggingBehaviorLog.MessageHandled(logger, useCase.Name, outcome, elapsed.TotalMilliseconds, user.UserId?.Value, user.RoleId?.Value);
            }
        }
    }

    // 将消息上下文写入日志作用域，便于期间所有日志自动附带这些属性
    // 仅记录标识与有限枚举值，避免记录姓名等个人敏感信息
    private Dictionary<string, object?> LogProperties(UseCase useCase)
    {
        var user = context.User;
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["Origin"] = context.Origin.ToString(),
            ["UserId"] = user.UserId?.Value,
            ["RoleId"] = user.RoleId?.Value,
            ["ActualUserId"] = user.ActualUserId?.Value,
            ["IsSystem"] = user.IsSystem,
            ["Module"] = useCase.Module,
            ["Message"] = useCase.Name,
        };
    }
}
