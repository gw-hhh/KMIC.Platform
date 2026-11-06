namespace Spike01.BuildingBlocks;

/// <summary>
/// 发送查询
/// </summary>
public interface IQuerySender
{
    ValueTask<Result<TResponse>> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken);
}
