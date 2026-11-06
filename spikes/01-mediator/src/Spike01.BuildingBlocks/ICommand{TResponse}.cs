namespace Spike01.BuildingBlocks;

/// <summary>
/// 返回值的命令
/// </summary>
public interface ICommand<TResponse> : Mediator.ICommand<Result<TResponse>>;
