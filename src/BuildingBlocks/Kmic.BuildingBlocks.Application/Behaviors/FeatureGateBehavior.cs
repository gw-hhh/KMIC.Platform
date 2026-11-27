using Kmic.BuildingBlocks.Application.Features;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Application.Behaviors;

/// <summary>检查用例声明的功能开关，未启用时直接拦截请求。</summary>
public sealed partial class FeatureGateBehavior<TMessage, TResponse>(UseCaseCatalog catalog, IFeatureFlags features, ILogger<FeatureGateBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : Result, IResultFactory<TResponse>
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        var useCase = catalog.Get(typeof(TMessage));
        if (useCase.FeatureGate is not { } gate || await features.IsEnabledAsync(gate, cancellationToken))
        {
            return await next(message, cancellationToken);
        }

        LogSkipped(logger, useCase.Name, gate);
        return TResponse.Fail(CommonErrors.FeatureDisabled);
    }

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Message {MessageName} skipped: feature {Feature} is off")]
    private static partial void LogSkipped(ILogger logger, string messageName, string feature);
}
