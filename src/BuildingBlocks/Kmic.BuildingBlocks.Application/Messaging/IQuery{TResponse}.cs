using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

public interface IQuery<TResponse> : Mediator.IQuery<Result<TResponse>>;
