namespace Spike01.BuildingBlocks;

/// <summary>
/// 查询处理器
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : Mediator.IQueryHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
