using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 命令发送
/// </summary>
/// <remarks>
/// 命令处理器不能注入它，命令里不能再发命令
/// </remarks>
public interface ICommandSender
{
    /// <summary>
    /// 发送不返回值的命令
    /// </summary>
    ValueTask<Result> SendAsync(ICommand command, CancellationToken ct);

    /// <summary>
    /// 发送有返回值的命令
    /// </summary>
    ValueTask<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct);
}
