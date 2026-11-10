using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 只读查询；不经过幂等与事务行为
/// </summary>
public interface IQuery<TResponse> : Mediator.IQuery<Result<TResponse>>;
