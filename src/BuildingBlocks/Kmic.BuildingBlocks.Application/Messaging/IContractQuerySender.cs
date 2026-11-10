using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// Contracts 接口的实现（薄适配器）发送本模块查询的入口
/// </summary>
/// <remarks>
/// 发送期间调用来源切换为 ModuleCall，身份沿用调用方；授权按查询上的 [ContractQuery] 声明（05-07 D2、D3）
/// </remarks>
public interface IContractQuerySender
{
    /// <summary>
    /// 以 ModuleCall 来源发送查询
    /// </summary>
    ValueTask<Result<TResponse>> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct);
}
