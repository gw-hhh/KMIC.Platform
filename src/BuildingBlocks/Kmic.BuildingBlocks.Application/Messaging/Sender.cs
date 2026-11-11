using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 消息发送
/// </summary>
internal sealed class Sender(Mediator.ISender mediator, ExecutionScope scope) : ISender
{
    public ValueTask<Result> SendAsync(ICommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        return SendCommandAsync(() => mediator.Send(command, ct));
    }

    public ValueTask<Result<TResponse>> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        return SendCommandAsync(() => mediator.Send(command, ct));
    }

    public ValueTask<Result<TResponse>> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        return mediator.Send(query, ct);
    }

    private async ValueTask<TResult> SendCommandAsync<TResult>(Func<ValueTask<TResult>> send)
    {
        // 一个请求只执行一个命令，命令里再发命令是代码写错了
        using var marker = scope.EnterCommand();
        return await send();
    }
}
