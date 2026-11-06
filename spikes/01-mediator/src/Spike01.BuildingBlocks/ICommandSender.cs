namespace Spike01.BuildingBlocks;

/// <summary>
/// 发送命令
/// </summary>
public interface ICommandSender
{
    ValueTask<Result> SendAsync(ICommand command, CancellationToken cancellationToken);

    ValueTask<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken);
}
