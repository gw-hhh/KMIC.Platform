namespace Spike01.BuildingBlocks;

/// <summary>
/// Contracts 适配器发送本模块查询的入口：调用期间来源为 ModuleCall，身份沿用调用方
/// </summary>
public interface IContractQuerySender
{
    Task<Result<TResponse>> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken);
}
