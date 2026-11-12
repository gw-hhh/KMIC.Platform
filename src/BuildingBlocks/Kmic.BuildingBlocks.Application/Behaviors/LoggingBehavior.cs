using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Observability;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Application.Behaviors;

/// <summary>
/// 请求日志
/// </summary>
/// <remarks>
/// 不记录消息内容，里面可能有敏感字段。排查时按追踪 ID 找请求日志
/// </remarks>
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
    /// <inheritdoc />
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

    // 处理这条消息期间的所有日志都带这些字段（Serilog 把日志作用域里的键值当作属性输出），业务代码不用自己传。
    // 只放 ID 和有限取值，不放姓名等个人信息
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
