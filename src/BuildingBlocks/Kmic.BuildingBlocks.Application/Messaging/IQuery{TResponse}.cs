using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 查询
/// </summary>
public interface IQuery<TResponse> : Mediator.IQuery<Result<TResponse>>;
