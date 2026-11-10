using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 发送查询；命令处理器可以注入，但查询看不到本命令尚未提交的修改
/// </summary>
public interface IQuerySender
{
    /// <summary>
    /// 发送查询
    /// </summary>
    ValueTask<Result<TResponse>> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct);
}
