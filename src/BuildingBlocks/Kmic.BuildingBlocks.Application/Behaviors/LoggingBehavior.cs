using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Application.Behaviors;

/// <summary>
/// 管道第 1 位：为每个消息建立 Activity，记录一条结果日志，超过阈值记警告
/// </summary>
/// <remarks>
/// 不记录消息内容：可能含敏感字段，排查时按追踪 ID 关联请求日志
/// </remarks>
public sealed class LoggingBehavior<TMessage, TResponse>(
    UseCaseCatalog catalog,
    IExecutionContext context,
    TimeProvider time,
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
}
