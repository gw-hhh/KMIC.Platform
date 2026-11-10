using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 发送命令；命令处理器不得注入它（命令内不能再发命令，05-02 D5）
/// </summary>
public interface ICommandSender
{
    /// <summary>
    /// 发送不返回值的命令
    /// </summary>
    ValueTask<Result> SendAsync(ICommand command, CancellationToken ct);

    /// <summary>
    /// 发送返回收据的命令
    /// </summary>
    ValueTask<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct);
}
