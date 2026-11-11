using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 不返回值的命令
/// </summary>
public interface ICommand : Mediator.ICommand<Result>;
