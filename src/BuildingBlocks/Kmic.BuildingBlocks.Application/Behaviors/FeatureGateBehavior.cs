using Kmic.BuildingBlocks.Application.Features;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Kmic.BuildingBlocks.Application.Behaviors;

/// <summary>
/// 功能开关检查
/// </summary>
/// <remarks>
/// 放在校验和授权之前：功能关闭时对所有人都是“不存在”，不泄露它有没有、谁能用。
/// 没有声明 <see cref="FeatureGateAttribute"/> 的用例只多一次目录查找
/// </remarks>
public sealed partial class FeatureGateBehavior<TMessage, TResponse>(UseCaseCatalog catalog, IFeatureFlags features, ILogger<FeatureGateBehavior<TMessage, TResponse>> logger)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : Result, IResultFactory<TResponse>
{
    /// <inheritdoc />
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
