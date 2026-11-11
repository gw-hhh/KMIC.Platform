using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// Contracts 查询发送
/// </summary>
/// <remarks>
/// 发送期间请求来源是 ModuleCall，身份沿用调用方，授权按查询上的 [ContractQuery]
/// </remarks>
public interface IContractQuerySender
{
    /// <summary>
    /// 发送查询
    /// </summary>
    ValueTask<Result<TResponse>> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct);
}
