namespace Spike01.BuildingBlocks;

/// <summary>
/// 查询
/// </summary>
public interface IQuery<TResponse> : Mediator.IQuery<Result<TResponse>>;
