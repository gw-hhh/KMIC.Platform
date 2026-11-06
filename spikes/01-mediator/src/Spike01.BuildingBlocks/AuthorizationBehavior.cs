using System.Reflection;
using Mediator;

namespace Spike01.BuildingBlocks;

/// <summary>
/// 管道第 4 位：全部消息；Contracts 查询只能以 ModuleCall 来源执行（05-07 第 4.3 节）
/// </summary>
public sealed class AuthorizationBehavior<TMessage, TResponse>(PipelineTrace trace, IExecutionContext context) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : Result, IResultFactory<TResponse>
{
    private static readonly bool _isContractQuery = typeof(TMessage).GetCustomAttribute<ContractQueryAttribute>() is not null;

    public ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        trace.Add($"Authorization:{typeof(TMessage).Name}:{context.Origin}");
        if (_isContractQuery && context.Origin != CallOrigin.ModuleCall)
        {
            return ValueTask.FromResult(TResponse.Fail(new Error("Common.ContractQueryOnlyFromModuleCall", "Contract queries can only be sent by module calls.")));
        }

        return next(message, cancellationToken);
    }
}
