using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// Contracts 查询发送
/// </summary>
public interface IContractQuerySender
{
    /// <summary>
    /// 发送查询
    /// </summary>
    ValueTask<Result<TResponse>> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct);
}
