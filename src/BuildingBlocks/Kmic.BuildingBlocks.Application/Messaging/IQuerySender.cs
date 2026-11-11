using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 查询发送
/// </summary>
/// <remarks>
/// 命令处理器可以注入它，但查不到本命令还没保存的修改
/// </remarks>
public interface IQuerySender
{
    /// <summary>
    /// 发送查询
    /// </summary>
    ValueTask<Result<TResponse>> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct);
}
