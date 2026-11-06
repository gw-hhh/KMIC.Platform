namespace Spike01.BuildingBlocks;

/// <summary>
/// 返回值的命令处理器
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : Mediator.ICommandHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
