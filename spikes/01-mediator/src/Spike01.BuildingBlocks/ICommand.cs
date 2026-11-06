namespace Spike01.BuildingBlocks;

/// <summary>
/// 不返回值的命令
/// </summary>
public interface ICommand : Mediator.ICommand<Result>;
