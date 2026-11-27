using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

public interface ICommand : Mediator.ICommand<Result>;
