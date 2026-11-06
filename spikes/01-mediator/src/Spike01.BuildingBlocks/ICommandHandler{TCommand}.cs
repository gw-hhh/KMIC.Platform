namespace Spike01.BuildingBlocks;

/// <summary>
/// 不返回值的命令处理器
/// </summary>
public interface ICommandHandler<in TCommand> : Mediator.ICommandHandler<TCommand, Result>
    where TCommand : ICommand;
