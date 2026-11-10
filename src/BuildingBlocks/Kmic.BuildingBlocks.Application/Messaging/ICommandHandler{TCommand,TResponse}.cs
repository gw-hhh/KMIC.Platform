using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 返回收据的命令处理器
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : Mediator.ICommandHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
